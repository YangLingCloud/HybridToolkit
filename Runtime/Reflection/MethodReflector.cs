using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HybridToolkit.Reflection
{
    /// <summary>
    /// 参数中单个字段的反射信息
    /// </summary>
    public class ReflectedFieldInfo
    {
        /// <summary>
        /// 字段的反射信息
        /// </summary>
        public FieldInfo FieldInfo;

        /// <summary>
        /// 字段类型
        /// </summary>
        public Type FieldType;

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// 方法参数的反射信息，包含参数类型及其内部字段
    /// </summary>
    public class ReflectedParameterInfo
    {
        /// <summary>
        /// 参数的反射信息
        /// </summary>
        public ParameterInfo ParameterInfo;

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ParameterType;

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 该参数类型是否为简单类型（基元、字符串、枚举、Unity 内置值类型）
        /// </summary>
        public bool IsSimpleType;

        /// <summary>
        /// 参数类型中的公共实例字段列表（仅复杂类型有值）
        /// </summary>
        public List<ReflectedFieldInfo> Fields = new List<ReflectedFieldInfo>();
    }

    /// <summary>
    /// 标记了 <see cref="ReflectiveInvokeAttribute"/> 的方法的反射信息
    /// </summary>
    public class ReflectedMethodInfo
    {
        /// <summary>
        /// 方法的反射信息
        /// </summary>
        public MethodInfo MethodInfo;

        /// <summary>
        /// 显示标签
        /// </summary>
        public string Label;

        /// <summary>
        /// 方法所属类型
        /// </summary>
        public Type DeclaringType;

        /// <summary>
        /// 方法是否为静态方法
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// 方法参数的反射信息列表
        /// </summary>
        public List<ReflectedParameterInfo> Parameters = new List<ReflectedParameterInfo>();
    }

    /// <summary>
    /// 包含标记方法的类的反射信息
    /// </summary>
    public class ReflectedTypeInfo
    {
        /// <summary>
        /// 类类型
        /// </summary>
        public Type Type;

        /// <summary>
        /// 该类中所有标记了特性的方法列表
        /// </summary>
        public List<ReflectedMethodInfo> Methods = new List<ReflectedMethodInfo>();
    }

    /// <summary>
    /// 方法反射器，扫描程序集以获取所有标记了 <see cref="ReflectiveInvokeAttribute"/> 的方法，
    /// 并解析其参数类型及参数类型中字段的类型信息。
    /// </summary>
    public static class MethodReflector
    {
        private static readonly HashSet<Type> SimpleTypes = new HashSet<Type>
        {
            typeof(int), typeof(float), typeof(double), typeof(long),
            typeof(short), typeof(byte), typeof(bool), typeof(string),
            typeof(char), typeof(uint), typeof(ulong), typeof(ushort),
            typeof(sbyte), typeof(decimal)
        };

        private static readonly HashSet<Type> UnityValueTypes = new HashSet<Type>
        {
            typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector4),
            typeof(UnityEngine.Color), typeof(UnityEngine.Rect), typeof(UnityEngine.Bounds),
            typeof(UnityEngine.Quaternion), typeof(UnityEngine.Vector2Int), typeof(UnityEngine.Vector3Int),
            typeof(UnityEngine.RectInt), typeof(UnityEngine.BoundsInt)
        };

        /// <summary>
        /// 判断类型是否为简单类型（不需要展开字段的类型）
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns>是否为简单类型</returns>
        public static bool IsSimpleType(Type type)
        {
            if (type == null) return false;
            if (SimpleTypes.Contains(type)) return true;
            if (type.IsEnum) return true;
            if (UnityValueTypes.Contains(type)) return true;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return true;
            return false;
        }

        /// <summary>
        /// 扫描指定程序集中所有标记了 <see cref="ReflectiveInvokeAttribute"/> 的方法
        /// </summary>
        /// <param name="assembly">要扫描的程序集</param>
        /// <returns>包含反射信息的类型列表</returns>
        public static List<ReflectedTypeInfo> Scan(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var result = new List<ReflectedTypeInfo>();
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type == null) continue;

                var typeInfo = ScanType(type, flags);
                if (typeInfo != null)
                {
                    result.Add(typeInfo);
                }
            }

            return result;
        }

        // ── 缓存 ──
        private static List<ReflectedTypeInfo> _cache;
        private static int _cacheAssemblyCount;

        /// <summary>
        /// 清除扫描缓存，下次调用 ScanAll 将重新扫描
        /// </summary>
        public static void ClearCache()
        {
            _cache = null;
            _cacheAssemblyCount = 0;
        }

        /// <summary>
        /// 扫描当前 AppDomain 中所有可能包含 <see cref="ReflectiveInvokeAttribute"/> 的程序集。
        /// <para>性能优化：仅扫描引用了 ReflectiveInvokeAttribute 所在程序集的程序集，并缓存结果。</para>
        /// </summary>
        /// <returns>包含反射信息的类型列表</returns>
        public static List<ReflectedTypeInfo> ScanAll()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 如果程序集数量未变化且缓存存在，直接返回缓存
            if (_cache != null && _cacheAssemblyCount == assemblies.Length)
                return _cache;

            var result = new List<ReflectedTypeInfo>();
            var attrAssemblyName = typeof(ReflectiveInvokeAttribute).Assembly.GetName().Name;

            for (int i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];

                // 跳过动态程序集
                if (asm.IsDynamic) continue;

                // 只扫描引用了 ReflectiveInvokeAttribute 所在程序集的程序集，或就是该程序集本身
                if (!ReferencesAssembly(asm, attrAssemblyName)) continue;

                var scanned = Scan(asm);
                result.AddRange(scanned);
            }

            _cache = result;
            _cacheAssemblyCount = assemblies.Length;
            return result;
        }

        /// <summary>
        /// 检查程序集是否引用了指定名称的程序集
        /// </summary>
        private static bool ReferencesAssembly(Assembly assembly, string targetName)
        {
            // 自身就是目标程序集
            if (assembly.GetName().Name == targetName) return true;

            var refs = assembly.GetReferencedAssemblies();
            for (int i = 0; i < refs.Length; i++)
            {
                if (refs[i].Name == targetName) return true;
            }
            return false;
        }

        /// <summary>
        /// 扫描单个类型
        /// </summary>
        /// <param name="type">要扫描的类型</param>
        /// <param name="flags">绑定标志</param>
        /// <returns>如果该类型包含标记方法则返回反射信息，否则返回 null</returns>
        public static ReflectedTypeInfo? ScanType(Type type, BindingFlags flags)
        {
            ReflectedTypeInfo? typeInfo = null;
            var methods = type.GetMethods(flags);

            for (int j = 0; j < methods.Length; j++)
            {
                var method = methods[j];
                var attr = method.GetCustomAttribute<ReflectiveInvokeAttribute>();
                if (attr == null) continue;

                if (typeInfo == null)
                {
                    typeInfo = new ReflectedTypeInfo { Type = type };
                }

                var methodInfo = BuildMethodInfo(method, attr);
                typeInfo.Methods.Add(methodInfo);
            }

            return typeInfo;
        }

        /// <summary>
        /// 构建方法反射信息
        /// </summary>
        private static ReflectedMethodInfo BuildMethodInfo(MethodInfo method, ReflectiveInvokeAttribute attr)
        {
            var methodInfo = new ReflectedMethodInfo
            {
                MethodInfo = method,
                Label = string.IsNullOrEmpty(attr.Label) ? method.Name : attr.Label,
                DeclaringType = method.DeclaringType,
                IsStatic = method.IsStatic
            };

            var parameters = method.GetParameters();
            for (int k = 0; k < parameters.Length; k++)
            {
                var param = parameters[k];
                var paramInfo = BuildParameterInfo(param);
                methodInfo.Parameters.Add(paramInfo);
            }

            return methodInfo;
        }

        /// <summary>
        /// 构建参数反射信息，包含参数类型内部的字段信息
        /// </summary>
        private static ReflectedParameterInfo BuildParameterInfo(ParameterInfo param)
        {
            var paramType = param.ParameterType;
            bool isSimple = IsSimpleType(paramType);

            var paramInfo = new ReflectedParameterInfo
            {
                ParameterInfo = param,
                ParameterType = paramType,
                Name = param.Name,
                IsSimpleType = isSimple
            };

            // 对于复杂类型，反射出其公共实例字段
            if (!isSimple && !paramType.IsAbstract && !paramType.IsInterface)
            {
                var fields = paramType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    paramInfo.Fields.Add(new ReflectedFieldInfo
                    {
                        FieldInfo = field,
                        FieldType = field.FieldType,
                        Name = field.Name
                    });
                }
            }

            return paramInfo;
        }
    }
}

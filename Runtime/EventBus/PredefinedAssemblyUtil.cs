using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 预定义程序集工具类，提供与预定义程序集交互的方法。
/// 它允许获取当前AppDomain中实现特定接口类型的所有类型。
/// 有关更多详细信息，请访问<see href="https://docs.unity3d.com/2023.3/Documentation/Manual/ScriptCompileOrderFolders.html">Unity文档</see>
/// </summary>
public static class PredefinedAssemblyUtil {
    /// <summary>
    /// 定义用于导航的特定预定义程序集类型的枚举。
    /// </summary>    
    enum AssemblyType {
        AssemblyCSharp,
        AssemblyCSharpEditor,
        AssemblyCSharpEditorFirstPass,
        AssemblyCSharpFirstPass
    }

    /// <summary>
    /// 将程序集名称映射到相应的AssemblyType。
    /// </summary>
    /// <param name="assemblyName">程序集的名称。</param>
    /// <returns>与程序集名称对应的AssemblyType，如果没有匹配则为null。</returns>
    static AssemblyType? GetAssemblyType(string assemblyName) {
        return assemblyName switch {
            "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
            "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
            "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
            "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
            _ => null
        };
    }

    /// <summary>
    /// 此方法遍历给定程序集，并将满足特定接口的类型添加到提供的集合中。
    /// </summary>
    /// <param name="assemblyTypes">表示程序集中所有类型的Type对象数组。</param>
    /// <param name="interfaceType">表示要检查的接口的Type。</param>
    /// <param name="results">应添加结果的类型集合。</param>
    static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> results) {
        if (assemblyTypes == null) return;
        for (int i = 0; i < assemblyTypes.Length; i++) {
            Type type = assemblyTypes[i];
            if (type != interfaceType && interfaceType.IsAssignableFrom(type)) {
                results.Add(type);
            }
        }
    }
    
    /// <summary>
    /// 获取当前AppDomain中所有实现提供的接口类型的类型。
    /// </summary>
    /// <param name="interfaceType">要获取所有类型的接口类型。</param>
    /// <returns>实现提供的接口类型的类型列表。</returns>    
    public static List<Type> GetTypes(Type interfaceType) {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
        List<Type> types = new List<Type>();
        for (int i = 0; i < assemblies.Length; i++) {
            AssemblyType? assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
            if (assemblyType != null) {
                assemblyTypes.Add((AssemblyType) assemblyType, assemblies[i].GetTypes());
            }
        }
        
        assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
        AddTypesFromAssembly(assemblyCSharpTypes, interfaceType, types);

        assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharpFirstPass, out var assemblyCSharpFirstPassTypes);
        AddTypesFromAssembly(assemblyCSharpFirstPassTypes, interfaceType, types);
        
        return types;
    }
}

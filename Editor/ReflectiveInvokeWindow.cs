using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using HybridToolkit.Reflection;

namespace HybridToolkit.Editor
{
    /// <summary>
    /// 反射调用编辑器窗口，基于 UIToolkit 实现。
    /// <para>自动扫描所有标记了 <see cref="ReflectiveInvokeAttribute"/> 的方法，
    /// 显示参数 UI 并支持在编辑器中直接执行方法。</para>
    /// </summary>
    public class ReflectiveInvokeWindow : EditorWindow
    {
        #region 颜色常量

        private static readonly Color ColorHeaderBg        = new Color(0.18f, 0.18f, 0.22f, 1f);
        private static readonly Color ColorHeaderText      = new Color(0.85f, 0.9f, 1f, 1f);
        private static readonly Color ColorTypeBg          = new Color(0.16f, 0.16f, 0.2f, 1f);
        private static readonly Color ColorTypeBorder      = new Color(0.3f, 0.55f, 0.85f, 0.6f);
        private static readonly Color ColorMethodBg        = new Color(0.14f, 0.14f, 0.17f, 1f);
        private static readonly Color ColorMethodBorder    = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color ColorExecAll         = new Color(0.22f, 0.55f, 0.35f, 1f);
        private static readonly Color ColorExecAllHover    = new Color(0.28f, 0.65f, 0.42f, 1f);
        private static readonly Color ColorExecMethod      = new Color(0.25f, 0.45f, 0.7f, 1f);
        private static readonly Color ColorExecMethodHover = new Color(0.3f, 0.55f, 0.82f, 1f);
        private static readonly Color ColorSignature       = new Color(0.55f, 0.6f, 0.7f, 1f);
        private static readonly Color ColorBadgeStatic     = new Color(0.4f, 0.7f, 0.95f, 0.18f);
        private static readonly Color ColorBadgeStaticTxt  = new Color(0.5f, 0.78f, 1f, 1f);
        private static readonly Color ColorBadgeInst       = new Color(0.9f, 0.65f, 0.3f, 0.18f);
        private static readonly Color ColorBadgeInstTxt    = new Color(0.95f, 0.75f, 0.4f, 1f);
        private static readonly Color ColorEmptyText       = new Color(0.5f, 0.5f, 0.55f, 1f);
        private static readonly Color ColorCountBadge      = new Color(1f, 1f, 1f, 0.12f);
        private static readonly Color ColorSeparator       = new Color(1f, 1f, 1f, 0.06f);

        #endregion

        #region 状态

        private ScrollView _scrollView;
        private Label _statsLabel;
        private List<ReflectedTypeInfo> _typeInfos;
        private readonly Dictionary<string, object> _parameterValues = new Dictionary<string, object>();
        private readonly Dictionary<string, UnityEngine.Object> _targetObjects = new Dictionary<string, UnityEngine.Object>();

        #endregion

        [MenuItem("HybridToolkit/反射调用窗口")]
        public static void ShowWindow()
        {
            var window = GetWindow<ReflectiveInvokeWindow>();
            window.titleContent = new GUIContent("反射调用窗口");
            window.minSize = new Vector2(460, 360);
        }

        #region UI 构建入口

        /// <summary>
        /// UIToolkit 入口，窗口打开时自动扫描并构建 UI
        /// </summary>
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);

            // ── 顶部标题栏 ──
            root.Add(BuildHeader());

            // ── 滚动视图 ──
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            _scrollView.style.paddingTop = 8;
            _scrollView.style.paddingBottom = 8;
            _scrollView.style.paddingLeft = 10;
            _scrollView.style.paddingRight = 10;
            root.Add(_scrollView);

            // 自动扫描
            ScanAndBuild();
        }

        /// <summary>
        /// 构建窗口顶部标题栏
        /// </summary>
        private VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.style.backgroundColor = ColorHeaderBg;
            header.style.paddingTop = 12;
            header.style.paddingBottom = 12;
            header.style.paddingLeft = 16;
            header.style.paddingRight = 16;
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = ColorSeparator;

            // 左侧标题
            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;

            var title = new Label("Reflective Invoke");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = ColorHeaderText;
            title.style.marginRight = 8;
            titleRow.Add(title);

            var subtitle = new Label("方法反射调用面板");
            subtitle.style.fontSize = 11;
            subtitle.style.color = new Color(0.55f, 0.58f, 0.65f, 1f);
            subtitle.style.marginTop = 2;
            titleRow.Add(subtitle);

            header.Add(titleRow);

            // 右侧统计
            _statsLabel = new Label();
            _statsLabel.style.fontSize = 10;
            _statsLabel.style.color = new Color(0.45f, 0.48f, 0.55f, 1f);
            header.Add(_statsLabel);

            return header;
        }

        #endregion

        #region 扫描

        /// <summary>
        /// 扫描所有程序集并构建 UI
        /// </summary>
        private void ScanAndBuild()
        {
            _scrollView.Clear();
            _typeInfos = MethodReflector.ScanAll();

            int totalMethods = 0;
            for (int i = 0; i < _typeInfos.Count; i++)
                totalMethods += _typeInfos[i].Methods.Count;

            _statsLabel.text = $"{_typeInfos.Count} 个类  ·  {totalMethods} 个方法";

            if (_typeInfos.Count == 0)
            {
                _scrollView.Add(BuildEmptyState());
                return;
            }

            for (int i = 0; i < _typeInfos.Count; i++)
                _scrollView.Add(BuildTypeCard(_typeInfos[i]));
        }

        #endregion

        #region 空状态

        /// <summary>
        /// 构建无结果时的空状态提示
        /// </summary>
        private VisualElement BuildEmptyState()
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            container.style.paddingTop = 60;

            var icon = new Label("?");
            icon.style.fontSize = 36;
            icon.style.marginBottom = 12;
            container.Add(icon);

            var text = new Label("未找到标记了 [ReflectiveInvoke] 的方法");
            text.style.fontSize = 13;
            text.style.color = ColorEmptyText;
            text.style.marginBottom = 6;
            container.Add(text);

            var hint = new Label("在方法上添加 [ReflectiveInvoke(\"标签\")] 特性即可");
            hint.style.fontSize = 11;
            hint.style.color = new Color(0.4f, 0.42f, 0.48f, 1f);
            container.Add(hint);

            return container;
        }

        #endregion

        #region 类卡片

        /// <summary>
        /// 为单个类构建可折叠的卡片式 UI 容器。
        /// <para>标题行包含折叠箭头、类名、方法数徽章、"执行所有"按钮。</para>
        /// <para>展开时延迟构建方法卡片（首次展开才创建子 UI，避免一次性构建所有方法的性能开销）。</para>
        /// </summary>
        private VisualElement BuildTypeCard(ReflectedTypeInfo typeInfo)
        {
            string typeKey = typeInfo.Type.FullName ?? typeInfo.Type.Name;

            // 外层卡片
            var card = new VisualElement();
            card.style.marginBottom = 10;
            card.style.backgroundColor = ColorTypeBg;
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            card.style.borderLeftWidth = 2;
            card.style.borderLeftColor = ColorTypeBorder;
            card.style.overflow = Overflow.Hidden;

            // ── 可点击标题行 ──
            var titleBar = new VisualElement();
            titleBar.style.flexDirection = FlexDirection.Row;
            titleBar.style.alignItems = Align.Center;
            titleBar.style.paddingTop = 8;
            titleBar.style.paddingBottom = 8;
            titleBar.style.paddingLeft = 10;
            titleBar.style.paddingRight = 10;
            titleBar.style.cursor = StyleKeyword.Auto;

            // 折叠箭头
            var arrow = new Label("\u25B6"); // ▶
            arrow.style.fontSize = 10;
            arrow.style.color = new Color(0.55f, 0.58f, 0.65f, 1f);
            arrow.style.marginRight = 6;
            arrow.style.width = 14;
            arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleBar.Add(arrow);

            // 类名
            var className = new Label(typeInfo.Type.Name);
            className.style.fontSize = 13;
            className.style.unityFontStyleAndWeight = FontStyle.Bold;
            className.style.color = new Color(0.85f, 0.88f, 0.95f, 1f);
            titleBar.Add(className);

            // 命名空间
            if (!string.IsNullOrEmpty(typeInfo.Type.Namespace))
            {
                var nsLabel = new Label($"  ({typeInfo.Type.Namespace})");
                nsLabel.style.fontSize = 10;
                nsLabel.style.color = new Color(0.42f, 0.45f, 0.52f, 1f);
                titleBar.Add(nsLabel);
            }

            // 弹性占位
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            titleBar.Add(spacer);

            // 方法数量徽章
            var countBadge = new Label($"{typeInfo.Methods.Count}");
            countBadge.style.fontSize = 10;
            countBadge.style.color = new Color(0.55f, 0.58f, 0.65f, 1f);
            countBadge.style.backgroundColor = ColorCountBadge;
            countBadge.style.borderTopLeftRadius = 8;
            countBadge.style.borderTopRightRadius = 8;
            countBadge.style.borderBottomLeftRadius = 8;
            countBadge.style.borderBottomRightRadius = 8;
            countBadge.style.paddingLeft = 7;
            countBadge.style.paddingRight = 7;
            countBadge.style.paddingTop = 2;
            countBadge.style.paddingBottom = 2;
            countBadge.style.marginRight = 8;
            titleBar.Add(countBadge);

            // "执行所有"按钮（在标题行上，不需要展开即可点击）
            var execAllBtn = new Button(() => ExecuteAllMethods(typeInfo)) { text = "全部执行" };
            StyleButton(execAllBtn, ColorExecAll, ColorExecAllHover);
            execAllBtn.style.height = 22;
            execAllBtn.style.paddingLeft = 10;
            execAllBtn.style.paddingRight = 10;
            execAllBtn.style.fontSize = 10;
            titleBar.Add(execAllBtn);

            card.Add(titleBar);

            // ── 可折叠内容区域 ──
            var contentContainer = new VisualElement();
            contentContainer.style.display = DisplayStyle.None; // 默认折叠
            contentContainer.style.paddingLeft = 14;
            contentContainer.style.paddingRight = 14;
            contentContainer.style.paddingTop = 6;
            contentContainer.style.paddingBottom = 10;
            contentContainer.style.borderTopWidth = 1;
            contentContainer.style.borderTopColor = ColorSeparator;
            card.Add(contentContainer);

            // 折叠状态 + 延迟构建
            bool isExpanded = false;
            bool isBuilt = false;

            // 点击标题行切换折叠（排除按钮区域的点击）
            titleBar.RegisterCallback<ClickEvent>(evt =>
            {
                // 如果点击的是按钮或按钮内部元素，不切换折叠
                if (evt.target is Button) return;
                var target = evt.target as VisualElement;
                while (target != null && target != titleBar)
                {
                    if (target is Button) return;
                    target = target.parent;
                }

                isExpanded = !isExpanded;
                contentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                arrow.text = isExpanded ? "\u25BC" : "\u25B6"; // ▼ / ▶

                // 延迟构建：首次展开时才创建方法 UI
                if (isExpanded && !isBuilt)
                {
                    isBuilt = true;
                    BuildTypeContent(contentContainer, typeInfo, typeKey);
                }
            });

            return card;
        }

        /// <summary>
        /// 构建类卡片的展开内容（实例目标 + 方法列表），仅在首次展开时调用
        /// </summary>
        private void BuildTypeContent(VisualElement container, ReflectedTypeInfo typeInfo, string typeKey)
        {
            // ── 实例方法目标对象 ──
            bool hasInstanceMethods = false;
            for (int i = 0; i < typeInfo.Methods.Count; i++)
            {
                if (!typeInfo.Methods[i].IsStatic) { hasInstanceMethods = true; break; }
            }

            if (hasInstanceMethods && typeof(UnityEngine.Object).IsAssignableFrom(typeInfo.Type))
            {
                var objField = new ObjectField("目标对象") { objectType = typeInfo.Type };
                if (_targetObjects.TryGetValue(typeKey, out var existing))
                    objField.value = existing;
                objField.RegisterValueChangedCallback(evt => _targetObjects[typeKey] = evt.newValue);
                objField.style.marginBottom = 6;
                container.Add(objField);
            }

            // ── 方法列表 ──
            for (int i = 0; i < typeInfo.Methods.Count; i++)
                container.Add(BuildMethodCard(typeInfo, typeInfo.Methods[i]));
        }

        #endregion

        #region 方法卡片

        /// <summary>
        /// 为单个方法构建参数 UI 和执行按钮
        /// </summary>
        private VisualElement BuildMethodCard(ReflectedTypeInfo typeInfo, ReflectedMethodInfo methodInfo)
        {
            string typeKey = typeInfo.Type.FullName ?? typeInfo.Type.Name;

            var card = new VisualElement();
            card.style.marginBottom = 6;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.backgroundColor = ColorMethodBg;
            card.style.borderTopLeftRadius = 4;
            card.style.borderTopRightRadius = 4;
            card.style.borderBottomLeftRadius = 4;
            card.style.borderBottomRightRadius = 4;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = ColorMethodBorder;
            card.style.borderBottomColor = ColorMethodBorder;
            card.style.borderLeftColor = ColorMethodBorder;
            card.style.borderRightColor = ColorMethodBorder;

            // ── 标题行 ──
            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;
            titleRow.style.marginBottom = 4;

            var methodLabel = new Label(methodInfo.Label);
            methodLabel.style.fontSize = 12;
            methodLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            methodLabel.style.color = new Color(0.82f, 0.85f, 0.92f, 1f);
            methodLabel.style.marginRight = 6;
            titleRow.Add(methodLabel);

            // static / instance 徽章
            bool isStatic = methodInfo.IsStatic;
            var badge = new Label(isStatic ? "static" : "instance");
            badge.style.fontSize = 9;
            badge.style.paddingLeft = 6;
            badge.style.paddingRight = 6;
            badge.style.paddingTop = 1;
            badge.style.paddingBottom = 1;
            badge.style.borderTopLeftRadius = 4;
            badge.style.borderTopRightRadius = 4;
            badge.style.borderBottomLeftRadius = 4;
            badge.style.borderBottomRightRadius = 4;
            badge.style.backgroundColor = isStatic ? ColorBadgeStatic : ColorBadgeInst;
            badge.style.color = isStatic ? ColorBadgeStaticTxt : ColorBadgeInstTxt;
            titleRow.Add(badge);

            card.Add(titleRow);

            // ── 签名 ──
            string sig = BuildSignature(methodInfo);
            var sigLabel = new Label(sig);
            sigLabel.style.fontSize = 10;
            sigLabel.style.color = ColorSignature;
            sigLabel.style.marginBottom = 6;
            sigLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            card.Add(sigLabel);

            // ── 参数区域 ──
            if (methodInfo.Parameters.Count > 0)
            {
                var paramsContainer = new VisualElement();
                paramsContainer.style.marginBottom = 6;

                for (int i = 0; i < methodInfo.Parameters.Count; i++)
                {
                    var paramInfo = methodInfo.Parameters[i];
                    string paramKey = $"{typeKey}.{methodInfo.MethodInfo.Name}.{paramInfo.Name}";

                    if (paramInfo.IsSimpleType)
                    {
                        var field = CreateFieldForType(paramInfo.Name, paramInfo.ParameterType, paramKey);
                        if (field != null) paramsContainer.Add(field);
                    }
                    else
                    {
                        var paramFoldout = new Foldout
                        {
                            text = $"{paramInfo.Name}  ({paramInfo.ParameterType.Name})",
                            value = true
                        };
                        paramFoldout.style.marginTop = 2;
                        paramFoldout.style.marginBottom = 2;

                        EnsureComplexParameterValue(paramKey, paramInfo.ParameterType);

                        for (int j = 0; j < paramInfo.Fields.Count; j++)
                        {
                            var fi = paramInfo.Fields[j];
                            string fieldKey = $"{paramKey}.{fi.Name}";
                            var fieldElem = CreateFieldForType(fi.Name, fi.FieldType, fieldKey);
                            if (fieldElem != null) paramFoldout.Add(fieldElem);
                        }
                        paramsContainer.Add(paramFoldout);
                    }
                }
                card.Add(paramsContainer);
            }

            // ── 执行按钮 ──
            var execBtn = new Button(() => ExecuteMethod(typeInfo, methodInfo))
            {
                text = $"执行  {methodInfo.Label}"
            };
            StyleButton(execBtn, ColorExecMethod, ColorExecMethodHover);
            execBtn.style.height = 26;
            card.Add(execBtn);

            return card;
        }

        #endregion

        #region 按钮样式

        /// <summary>
        /// 为按钮应用统一的悬停高亮样式
        /// </summary>
        private void StyleButton(Button btn, Color normal, Color hover)
        {
            btn.style.backgroundColor = normal;
            btn.style.color = Color.white;
            btn.style.borderTopWidth = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth = 0;
            btn.style.borderRightWidth = 0;
            btn.style.borderTopLeftRadius = 4;
            btn.style.borderTopRightRadius = 4;
            btn.style.borderBottomLeftRadius = 4;
            btn.style.borderBottomRightRadius = 4;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.fontSize = 11;

            btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = hover);
            btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = normal);
        }

        #endregion

        #region 类型字段工厂

        /// <summary>
        /// 构建方法签名字符串
        /// </summary>
        private string BuildSignature(ReflectedMethodInfo methodInfo)
        {
            var parts = new List<string>();
            for (int i = 0; i < methodInfo.Parameters.Count; i++)
            {
                var p = methodInfo.Parameters[i];
                parts.Add($"{p.ParameterType.Name} {p.Name}");
            }
            return $"{methodInfo.MethodInfo.ReturnType.Name} {methodInfo.MethodInfo.Name}({string.Join(", ", parts)})";
        }

        /// <summary>
        /// 为指定类型创建对应的 UIToolkit 输入控件
        /// </summary>
        private VisualElement CreateFieldForType(string label, Type type, string key)
        {
            if (type == typeof(int))
            {
                int current = GetValue<int>(key);
                var field = new IntegerField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(float))
            {
                float current = GetValue<float>(key);
                var field = new FloatField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(double))
            {
                double current = GetValue<double>(key);
                var field = new DoubleField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(long))
            {
                long current = GetValue<long>(key);
                var field = new LongField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(string))
            {
                string current = GetValue<string>(key) ?? "";
                var field = new TextField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(bool))
            {
                bool current = GetValue<bool>(key);
                var field = new Toggle(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Vector2))
            {
                var current = GetValue<Vector2>(key);
                var field = new Vector2Field(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Vector3))
            {
                var current = GetValue<Vector3>(key);
                var field = new Vector3Field(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Vector4))
            {
                var current = GetValue<Vector4>(key);
                var field = new Vector4Field(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Color))
            {
                var current = GetValue<Color>(key, Color.white);
                var field = new ColorField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Rect))
            {
                var current = GetValue<Rect>(key);
                var field = new RectField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Bounds))
            {
                var current = GetValue<Bounds>(key);
                var field = new BoundsField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Vector2Int))
            {
                var current = GetValue<Vector2Int>(key);
                var field = new Vector2IntField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(Vector3Int))
            {
                var current = GetValue<Vector3Int>(key);
                var field = new Vector3IntField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(RectInt))
            {
                var current = GetValue<RectInt>(key);
                var field = new RectIntField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type == typeof(BoundsInt))
            {
                var current = GetValue<BoundsInt>(key);
                var field = new BoundsIntField(label) { value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (type.IsEnum)
            {
                Enum current;
                if (_parameterValues.TryGetValue(key, out var existing) && existing is Enum enumVal)
                    current = enumVal;
                else
                {
                    current = (Enum)Enum.ToObject(type, 0);
                    _parameterValues[key] = current;
                }
                var field = new EnumField(label, current);
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                UnityEngine.Object current = null;
                if (_parameterValues.TryGetValue(key, out var existing) && existing is UnityEngine.Object obj)
                    current = obj;
                var field = new ObjectField(label) { objectType = type, value = current };
                field.RegisterValueChangedCallback(evt => _parameterValues[key] = evt.newValue);
                return field;
            }

            var unsupported = new Label($"{label}: 不支持的类型 ({type.Name})");
            unsupported.style.color = new Color(1f, 0.5f, 0.5f);
            return unsupported;
        }

        private T GetValue<T>(string key, T defaultValue = default)
        {
            if (_parameterValues.TryGetValue(key, out var val) && val is T typed)
                return typed;
            _parameterValues[key] = defaultValue;
            return defaultValue;
        }

        private void EnsureComplexParameterValue(string paramKey, Type paramType)
        {
            if (!_parameterValues.ContainsKey(paramKey))
            {
                try { _parameterValues[paramKey] = Activator.CreateInstance(paramType); }
                catch { _parameterValues[paramKey] = null; }
            }
        }

        #endregion

        #region 参数收集与执行

        /// <summary>
        /// 从 UI 中收集方法参数值
        /// </summary>
        private object[] CollectArguments(ReflectedTypeInfo typeInfo, ReflectedMethodInfo methodInfo)
        {
            string typeKey = typeInfo.Type.FullName ?? typeInfo.Type.Name;
            var args = new object[methodInfo.Parameters.Count];

            for (int i = 0; i < methodInfo.Parameters.Count; i++)
            {
                var paramInfo = methodInfo.Parameters[i];
                string paramKey = $"{typeKey}.{methodInfo.MethodInfo.Name}.{paramInfo.Name}";

                if (paramInfo.IsSimpleType)
                {
                    _parameterValues.TryGetValue(paramKey, out args[i]);
                }
                else
                {
                    object instance;
                    try { instance = Activator.CreateInstance(paramInfo.ParameterType); }
                    catch { instance = null; }

                    if (instance != null)
                    {
                        for (int j = 0; j < paramInfo.Fields.Count; j++)
                        {
                            var fi = paramInfo.Fields[j];
                            string fieldKey = $"{paramKey}.{fi.Name}";
                            if (_parameterValues.TryGetValue(fieldKey, out var fieldVal))
                            {
                                try { fi.FieldInfo.SetValue(instance, fieldVal); }
                                catch (Exception e) { Debug.LogError($"[ReflectiveInvoke] 设置字段 {fi.Name} 失败: {e.Message}"); }
                            }
                        }
                    }
                    args[i] = instance;
                }
            }
            return args;
        }

        /// <summary>
        /// 获取实例方法的目标对象
        /// </summary>
        private object GetTargetInstance(ReflectedTypeInfo typeInfo)
        {
            string typeKey = typeInfo.Type.FullName ?? typeInfo.Type.Name;

            if (typeof(UnityEngine.Object).IsAssignableFrom(typeInfo.Type))
            {
                _targetObjects.TryGetValue(typeKey, out var target);
                return target;
            }
            try { return Activator.CreateInstance(typeInfo.Type); }
            catch (Exception e)
            {
                Debug.LogError($"[ReflectiveInvoke] 创建 {typeInfo.Type.Name} 实例失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 执行单个方法
        /// </summary>
        private void ExecuteMethod(ReflectedTypeInfo typeInfo, ReflectedMethodInfo methodInfo)
        {
            try
            {
                var args = CollectArguments(typeInfo, methodInfo);
                object target = null;

                if (!methodInfo.IsStatic)
                {
                    target = GetTargetInstance(typeInfo);
                    if (target == null)
                    {
                        Debug.LogError($"[ReflectiveInvoke] 执行失败：{typeInfo.Type.Name}.{methodInfo.MethodInfo.Name} 是实例方法，但未找到目标对象。");
                        return;
                    }
                }

                var result = methodInfo.MethodInfo.Invoke(target, args);

                if (methodInfo.MethodInfo.ReturnType != typeof(void))
                    Debug.Log($"[ReflectiveInvoke] {methodInfo.Label} 返回值: {result}");
                else
                    Debug.Log($"[ReflectiveInvoke] {methodInfo.Label} 执行完成");
            }
            catch (TargetInvocationException e)
            {
                Debug.LogError($"[ReflectiveInvoke] 执行 {methodInfo.Label} 时出错: {e.InnerException?.Message ?? e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReflectiveInvoke] 执行 {methodInfo.Label} 时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 执行该类的所有标记方法
        /// </summary>
        private void ExecuteAllMethods(ReflectedTypeInfo typeInfo)
        {
            Debug.Log($"[ReflectiveInvoke] 开始执行 {typeInfo.Type.Name} 的所有方法 ({typeInfo.Methods.Count} 个)");
            for (int i = 0; i < typeInfo.Methods.Count; i++)
                ExecuteMethod(typeInfo, typeInfo.Methods[i]);
            Debug.Log($"[ReflectiveInvoke] {typeInfo.Type.Name} 的所有方法执行完成");
        }

        #endregion
    }
}

using System;

namespace HybridToolkit
{
    /// <summary>
    /// 反射调用特性，标记的方法将在 ReflectiveInvokeWindow 编辑器窗口中显示。
    /// <para>支持通过反射获取方法参数类型及参数字段类型，并在编辑器中提供 UI 来填写参数并执行方法。</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ReflectiveInvokeAttribute : Attribute
    {
        /// <summary>
        /// 按钮显示名称，为空时使用方法名
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// 反射调用特性构造函数
        /// </summary>
        /// <param name="label">按钮显示名称，为空时使用方法名</param>
        public ReflectiveInvokeAttribute(string label = "")
        {
            Label = label;
        }
    }
}

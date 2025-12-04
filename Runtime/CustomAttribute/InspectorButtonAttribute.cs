using System;
using UnityEngine;

namespace HybridToolkit
{
    /// <summary>
    /// Inspector按钮特性，用于在Inspector中显示可点击的方法按钮
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InspectorButtonAttribute : PropertyAttribute
    {
        /// <summary>
        /// 按钮显示名称
        /// </summary>
        public string ButtonName { get; private set; }
        
        /// <summary>
        /// 是否显示在播放模式下
        /// </summary>
        public bool ShowInPlayMode { get; private set; }
        
        /// <summary>
        /// 是否显示在编辑模式下
        /// </summary>
        public bool ShowInEditMode { get; private set; }
        
        /// <summary>
        /// Inspector按钮特性构造函数
        /// </summary>
        /// <param name="buttonName">按钮显示名称</param>
        /// <param name="showInPlayMode">是否在播放模式下显示</param>
        /// <param name="showInEditMode">是否在编辑模式下显示</param>
        public InspectorButtonAttribute(string buttonName = "", bool showInPlayMode = true, bool showInEditMode = true)
        {
            ButtonName = buttonName;
            ShowInPlayMode = showInPlayMode;
            ShowInEditMode = showInEditMode;
        }
    }
}
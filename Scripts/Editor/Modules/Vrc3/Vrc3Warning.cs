#if VRC_SDK_VRCSDK3
using System;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class Vrc3Warning
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string Button;
        public readonly Action Action;
        public readonly bool Closable;

        public Vrc3Warning(string title, string description, bool closable = true, string button = null, Action action = null)
        {
            Title = title;
            Description = description;
            Closable = closable;
            Button = button;
            Action = action;
        }

        public static readonly Vrc3Warning PausedEditor = new("编辑器已暂停", "编辑器处于暂停状态，动画器也会一并暂停。", false);
        public static readonly Vrc3Warning InitLoadJsonError = new("初始化错误", "无法加载本地存储的参数。(JSON 格式错误)");
        public static readonly Vrc3Warning InitLoadUnexisting = new("初始化错误", "无法加载本地存储的参数。(文件不存在)");
    }
}
#endif
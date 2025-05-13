﻿using Drawie.Numerics;
using Drawie.Windowing.Input;
using JSRuntime = Drawie.JSInterop.JSRuntime;

namespace Drawie.Windowing.Browser;

public partial class BrowserInterop
{
    private static bool subscribedWindowResize = false;
    private static event Action<double> OnRender;

    static BrowserInterop()
    {
        JSRuntime.OnAnimationFrameCalled += OnAnimationFrame;
    }
    
    public static string GetTitle()
    {
        return JSRuntime.GetTitle();
    }
    public static void SetTitle(string value)
    {
        JSRuntime.InvokeJs($"document.title = '{value}'");
    }

    public static VecI GetWindowSize()
    {
        int width = JSRuntime.GetWindowWidth();
        int height = JSRuntime.GetWindowHeight();
        
        return new VecI(width, height);
    }

    public static void RequestAnimationFrame(Action<double> onRender)
    {
        OnRender += onRender;
        JSRuntime.RequestAnimationFrame();
    }

    private static void OnAnimationFrame(double obj)
    {
        OnRender?.Invoke(obj);
        OnRender = null;
    }

    public static void SubscribeWindowResize(Action<int, int> onWindowResize)
    {
        if (!subscribedWindowResize)
        {
            JSRuntime.SubscribeWindowResize();
            subscribedWindowResize = true;
        }
        
        JSRuntime.WindowResizedEvent += onWindowResize;
    }

    public static bool IsKeyPressed(Key key)
    {
        return false;
        //return JSRuntime.IsKeyPressed(key);
    }
}

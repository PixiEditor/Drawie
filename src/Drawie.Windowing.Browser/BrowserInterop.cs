using Drawie.Numerics;
using Drawie.Windowing.Input;
using JSRuntime = Drawie.JSInterop.JSRuntime;

namespace Drawie.Windowing.Browser;

public partial class BrowserInterop
{
    private static bool subscribedWindowResize = false;
    private static bool subscribedKeyDown = false;
    private static bool subscribedKeyUp = false;
    private static List<Action<double>>? OnRender = new List<Action<double>>();

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
        OnRender?.Add(onRender);
        JSRuntime.RequestAnimationFrame();
    }

    private static void OnAnimationFrame(double obj)
    {
        if (OnRender == null)
        {
            return;
        }

        int count = OnRender.Count;
        for (int i = 0; i < count; i++)
        {
            OnRender[i].Invoke(obj);
        }

        OnRender.RemoveRange(0, count);
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

    public static void SubscribeKeyDownEvent(Action<string> onKeyDown)
    {
        if (!subscribedKeyDown)
        {
            JSRuntime.SubscribeKeyDown();
            subscribedKeyDown = true;
        }

        JSRuntime.KeyDownEvent += onKeyDown;
    }

    public static void SubscribeKeyUpEvent(Action<string> onKeyUp)
    {
        if (!subscribedKeyUp)
        {
            JSRuntime.SubscribeKeyUp();
            subscribedKeyUp = true;
        }

        JSRuntime.KeyUpEvent += onKeyUp;
    }
}
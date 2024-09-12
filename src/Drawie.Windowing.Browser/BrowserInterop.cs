using System.Runtime.InteropServices.JavaScript;
using Drawie.JSInterop;
using PixiEditor.Numerics;

namespace Drawie.Windowing.Browser;

public partial class BrowserInterop
{
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
}
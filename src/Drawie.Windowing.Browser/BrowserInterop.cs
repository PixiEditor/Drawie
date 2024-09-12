using System.Runtime.InteropServices.JavaScript;
using Microsoft.JSInterop;

namespace Drawie.Windowing.Browser;

public partial class BrowserInterop
{
    [JSImport("interop.invokeJs", "main.js")]
    internal static partial void InvokeJs(string js);
    
    [JSImport("window.document.title", "main.js")]
    internal static partial string GetTitle();

    public static void SetTitle(string value)
    {
        InvokeJs($"document.title = '{value}'");
    }
}
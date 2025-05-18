using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    private static int nextId = 0;
    
    public static event Action<double> OnAnimationFrameCalled;
    public static event Action<int, int> WindowResizedEvent;
    public static event Action<string> KeyDownEvent;
    public static event Action<string> KeyUpEvent;


    [JSImport("interop.invokeJs", "drawie.js")]
    public static partial void InvokeJs(string js);

    [JSImport("window.document.title", "drawie.js")]
    public static partial string GetTitle();

    [JSImport("window.innerWidth", "drawie.js")]
    public static partial int GetWindowWidth();

    [JSImport("window.innerHeight", "drawie.js")]
    public static partial int GetWindowHeight();

    [JSImport("window.requestAnimationFrame", "drawie.js")]
    public static partial int RequestAnimationFrame();
    
    [JSImport("window.subscribeWindowResize", "drawie.js")]
    public static partial void SubscribeWindowResize();

    [JSImport("input.subscribeKeyDown", "drawie.js")]
    public static partial void SubscribeKeyDown();


    [JSImport("input.subscribeKeyUp", "drawie.js")]
    public static partial void SubscribeKeyUp();

    [JSExport]
    private static void OnKeyDown(string keyCode)
    {
        KeyDownEvent?.Invoke(keyCode);
    }

    [JSExport]
    private static void OnKeyUp(string keyCode)
    {
        KeyUpEvent?.Invoke(keyCode);
    }

    [JSExport]
    internal static void OnAnimationFrame(double dt)
    {
        OnAnimationFrameCalled?.Invoke(dt);
    }
    
    [JSExport]
    internal static void WindowResized(int width, int height)
    {
        WindowResizedEvent?.Invoke(width, height);
    }

    public static T CreateElement<T>() where T : HtmlObject, new()
    {
        int id = nextId;
        T obj = new T { Id = $"element_{id}" };
        // todo, don't use eval
        InvokeJs($"""
                  var element = document.createElement('{obj.TagName}');
                  element.id = 'element_{id}';
                  document.body.appendChild(element);
                  """);

        nextId++;
        return obj;
    }

    public static HtmlObject CreateElement(string tagName)
    {
        int id = nextId;
        // todo, don't use eval
        InvokeJs($"""
                  var element = document.createElement('{tagName}');
                  element.id = 'element_{id}';
                  document.body.appendChild(element);
                  """);
        HtmlObject obj = new HtmlObject(tagName) { Id = $"element_{id}" };
        nextId++;

        return obj;
    }
}

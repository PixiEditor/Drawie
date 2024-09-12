using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    private static int nextId = 0;
    [JSImport("interop.invokeJs", "main.js")]
    public static partial void InvokeJs(string js);
    
    [JSImport("window.document.title", "main.js")]
    public static partial string GetTitle();
    
    [JSImport("window.innerWidth", "main.js")]
    public static partial int GetWindowWidth();
    
    [JSImport("window.innerHeight", "main.js")]
    public static partial int GetWindowHeight();
    
    public static HtmlObject CreateElement(string tagName)
    {
        int id = nextId;
        InvokeJs($"""
                  var element = document.createElement('{tagName}');
                  element.id = 'element{id}';
                  document.body.appendChild(element);
                  """);
        HtmlObject obj = new HtmlObject { TagName = tagName, Id = id };
        nextId++;
        
        return obj;
    }
}
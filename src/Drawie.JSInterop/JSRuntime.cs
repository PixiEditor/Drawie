﻿using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    private static int nextId = 0;
    
    public static event Action<double> OnAnimationFrameCalled;

    [JSImport("interop.invokeJs", "main.js")]
    public static partial void InvokeJs(string js);

    [JSImport("window.document.title", "main.js")]
    public static partial string GetTitle();

    [JSImport("window.innerWidth", "main.js")]
    public static partial int GetWindowWidth();

    [JSImport("window.innerHeight", "main.js")]
    public static partial int GetWindowHeight();

    [JSImport("window.requestAnimationFrame", "main.js")]
    public static partial int RequestAnimationFrame();
    
    [JSExport]
    internal static void OnAnimationFrame(double dt)
    {
        OnAnimationFrameCalled?.Invoke(dt);
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
using Drawie.JSInterop;

namespace Drawie.RenderApi.Html5Canvas;

public class CanvasContext : IDisposable
{
    private int handle;
    private readonly HtmlCanvas canvas;
    
    internal CanvasContext(HtmlCanvas canvas)
    {
        this.canvas = canvas;
        handle = JSRuntime.OpenCanvasContext(canvas.Id);
        Console.WriteLine($"Opened canvas context with handle {handle}");
    }

    public static CanvasContext GetContext(HtmlCanvas canvas)
    {
        return new CanvasContext(canvas);
    }
    
    public void SetFillStyle(string color)
    {
        JSRuntime.InvokeJs($"canvasContextHandles[{handle}].fillStyle = '{color}'");
    }
    
    public void FillRect(int x, int y, int width, int height)
    {
        JSRuntime.InvokeJs($"canvasContextHandles[{handle}].fillRect({x}, {y}, {width}, {height})");
    }
    
    public void Dispose()
    {
            
    }
}
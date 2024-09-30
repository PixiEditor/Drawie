using Drawie.JSInterop;
using Drawie.Numerics;

namespace Drawie.Html5Canvas.Objects;

public class HtmlCanvasObject(string canvasId, VecI size) : HtmlObject
{
    public string CanvasId { get; } = canvasId;
    public VecI Size { get; } = size;
    
    public CanvasContext Context => activeContext ??= OpenContext();
    
    private CanvasContext activeContext;
    
    private CanvasContext OpenContext()
    {
        int ctxHandle = JSRuntime.OpenCanvasContext(CanvasId);
        return new CanvasContext(ctxHandle);
    }
}
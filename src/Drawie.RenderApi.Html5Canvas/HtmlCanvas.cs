using Drawie.JSInterop;

namespace Drawie.RenderApi.Html5Canvas;

public class HtmlCanvas() : HtmlObject("canvas"), ICanvasTexture
{
    public string CanvasId => Id; 
}

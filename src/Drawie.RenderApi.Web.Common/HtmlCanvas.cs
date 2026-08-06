using Drawie.JSInterop;

namespace Drawie.RenderApi.Web.Common;

public class HtmlCanvas() : HtmlObject("canvas"), ICanvasTexture
{
    public string CanvasId => Id; 
}

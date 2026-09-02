using Drawie.JSInterop;
using Drawie.Numerics;

namespace Drawie.RenderApi.Web.Common;

public class HtmlCanvas(VecI size) : HtmlObject("canvas"), ICanvasTexture
{
    public string CanvasId => Id;
    public ulong TextureId { get; } = ulong.MaxValue;
    public VecI Size { get; }
}

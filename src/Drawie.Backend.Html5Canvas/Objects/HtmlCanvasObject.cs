using PixiEditor.Numerics;

namespace Drawie.Html5Canvas.Objects;

public class HtmlCanvasObject(string canvasId, VecI size) : HtmlObject
{
    public string CanvasId { get; } = canvasId;
    public VecI Size { get; } = size;
}
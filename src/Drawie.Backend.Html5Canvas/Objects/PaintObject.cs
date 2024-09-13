using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace Drawie.Html5Canvas.Objects;

public class PaintObject
{
    public PaintStyle Style { get; set; }
    public BlendMode BlendMode { get; set; }
    public FilterQuality FilterQuality { get; set; }
    public Color Color { get; set; }
}
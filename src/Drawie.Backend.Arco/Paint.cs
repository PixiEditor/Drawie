using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;

namespace Drawie.Backend.Arco;

public class Paint
{
    public BlendMode BlendMode { get; set; } = BlendMode.SrcOver;
    public Color Color { get; set; }
}
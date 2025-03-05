using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public abstract class Paintable
{
    public abstract bool AnythingVisible { get; }
    public bool AbsoluteValues { get; set; } = false;
    public abstract Shader? GetShader(RectD bounds, Matrix3X3 matrix);

    public static implicit operator Paintable(Color color) => new ColorPaintable(color);
}

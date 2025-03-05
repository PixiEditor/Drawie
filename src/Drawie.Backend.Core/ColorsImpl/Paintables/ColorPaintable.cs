using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class ColorPaintable : Paintable
{
    public override bool AnythingVisible => Color.A > 0;

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        return null;
    }

    public Color Color { get; }

    public ColorPaintable(Color color)
    {
        Color = color;
    }
}

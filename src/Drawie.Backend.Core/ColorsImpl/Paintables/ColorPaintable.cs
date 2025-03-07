using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class ColorPaintable : Paintable
{
    public Color Color { get; private set; }
    public override bool AnythingVisible => Color.A > 0;

    public ColorPaintable(Color color)
    {
        Color = color;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        return null;
    }

    internal override Shader? GetShaderCached()
    {
        return null;
    }

    public override Paintable? Clone()
    {
        return new ColorPaintable(Color);
    }

    public override void ApplyOpacity(double opacity)
    {
        Color = Color.WithAlpha((byte)(Color.A * opacity));
    }
}

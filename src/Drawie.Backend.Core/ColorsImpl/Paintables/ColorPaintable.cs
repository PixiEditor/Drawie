using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces.PaintImpl;
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

    public override void ModifyPaint(Paint paint)
    {
        paint.Color = Color;
    }

    public override Paintable? Clone()
    {
        return new ColorPaintable(Color);
    }

    public override void ApplyOpacity(double opacity)
    {
        Color = Color.WithAlpha((byte)(Color.A * opacity));
    }

    protected bool Equals(ColorPaintable other)
    {
        return Color.Equals(other.Color);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ColorPaintable)obj);
    }

    public override int GetHashCode()
    {
        return Color.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Color}";
    }
}

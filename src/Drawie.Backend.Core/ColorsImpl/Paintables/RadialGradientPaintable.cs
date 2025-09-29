using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class RadialGradientPaintable : GradientPaintable, IPositionPaintable
{
    public VecD Center { get; set; }
    public double Radius { get; set; }

    VecD IPositionPaintable.Position
    {
        get => Center;
        set => Center = value;
    }

    public RadialGradientPaintable(VecD center, double radius, IEnumerable<GradientStop> gradientStops) : base(
        gradientStops)
    {
        Center = center;
        Radius = radius;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        Color[] colors = GradientStops.Select(x => x.Color).ToArray();
        float[] offsets = GradientStops.Select(x => (float)x.Offset).ToArray();

        Matrix3X3 finalMatrix = matrix;
        if (Transform != null)
        {
            finalMatrix = matrix.Concat(Transform.Value);
        }

        if (Bounds != null)
        {
            bounds = Bounds.Value;
        }

        VecD center = AbsoluteValues
            ? Center
            : new VecD(Center.X * bounds.Width + bounds.X, Center.Y * bounds.Height + bounds.Y);
        double radius = AbsoluteValues ? Radius : Radius * bounds.Width;
        return Shader.CreateRadialGradient(center, (float)radius, colors, offsets, finalMatrix);
    }

    public override Paintable? Clone()
    {
        return new RadialGradientPaintable(Center, Radius, GradientStops.Select(x => x))
        {
            AbsoluteValues = AbsoluteValues,
            Transform = Transform
        };
    }

    protected bool Equals(RadialGradientPaintable other)
    {
        return base.Equals(other) && Center.Equals(other.Center) && Radius.Equals(other.Radius);
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

        return Equals((RadialGradientPaintable)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Center, Radius);
    }
}

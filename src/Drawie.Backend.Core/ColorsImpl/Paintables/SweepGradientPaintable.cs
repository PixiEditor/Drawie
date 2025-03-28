using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class SweepGradientPaintable : GradientPaintable
{
    public VecD Center { get; set; } = new VecD(0.5, 0.5);
    public double Angle { get; set; }
    public SweepGradientPaintable(VecD center, double angle, IEnumerable<GradientStop> gradientStops) : base(gradientStops)
    {
        Center = center;
        Angle = angle;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        VecD finalCenter = AbsoluteValues ? Center : new VecD(Center.X * bounds.Width + bounds.X, Center.Y * bounds.Height + bounds.Y);
        lastShader = Shader.CreateSweepGradient(finalCenter,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray(),
            TileMode.Clamp,
            (float)Angle,
            matrix);
        return lastShader;
    }

    public override Paintable? Clone()
    {
        return new SweepGradientPaintable(Center, Angle, GradientStops.Select(x => x));
    }

    protected bool Equals(SweepGradientPaintable other)
    {
        return base.Equals(other) && Center.Equals(other.Center) && Angle.Equals(other.Angle);
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

        return Equals((SweepGradientPaintable)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Center, Angle);
    }
}

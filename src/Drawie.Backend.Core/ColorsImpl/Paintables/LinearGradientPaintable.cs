using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class LinearGradientPaintable : GradientPaintable, IStartEndPaintable
{
    public VecD Start { get; set; }
    public VecD End { get; set; }

    public LinearGradientPaintable(VecD start, VecD end, IEnumerable<GradientStop> gradientStops) : base(gradientStops)
    {
        Start = start;
        End = end;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        Matrix3X3 finalMatrix = matrix;

        if (Transform != null)
        {
            finalMatrix = matrix.Concat(Transform.Value);
        }

        if (Bounds != null)
        {
            bounds = Bounds.Value;
        }

        VecD start = AbsoluteValues
            ? Start
            : new VecD(Start.X * bounds.Width + bounds.X, Start.Y * bounds.Height + bounds.Y);
        VecD end = AbsoluteValues ? End : new VecD(End.X * bounds.Width + bounds.X, End.Y * bounds.Height + bounds.Y);
        return Shader.CreateLinearGradient(start, end,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray(),
            finalMatrix);
    }

    public override Paintable? Clone()
    {
        return new LinearGradientPaintable(Start, End, GradientStops.Select(x => x).ToList())
        {
            AbsoluteValues = AbsoluteValues,
            Transform = Transform
        };
    }

    protected bool Equals(LinearGradientPaintable other)
    {
        return base.Equals(other) && Start.Equals(other.Start) && End.Equals(other.End);
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

        return Equals((LinearGradientPaintable)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Start, End);
    }
}

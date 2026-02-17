using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public abstract class GradientPaintable : Paintable, ISrgbPaintable
{
    public override bool AnythingVisible => GradientStops is { Count: > 0 } && GradientStops.Any(x => x.Color.A > 0);
    public List<GradientStop> GradientStops { get; }
    public Matrix3X3? Transform { get; set; }

    public GradientPaintable(IEnumerable<GradientStop> gradientStops)
    {
        GradientStops = new List<GradientStop>(gradientStops);
    }

    public override void ApplyOpacity(double opacity)
    {
        List<GradientStop> newStops = new();
        foreach (GradientStop stop in GradientStops)
        {
            newStops.Add(new GradientStop(stop.Color.WithAlpha((byte)(stop.Color.A * opacity)), stop.Offset));
        }

        GradientStops.Clear();
        GradientStops.AddRange(newStops);
    }

    protected bool Equals(GradientPaintable other)
    {
        return (GradientStops == other.GradientStops || GradientStops.SequenceEqual(other.GradientStops)) && Nullable.Equals(Transform, other.Transform);
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

        return Equals((GradientPaintable)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GradientStops, Transform);
    }
}

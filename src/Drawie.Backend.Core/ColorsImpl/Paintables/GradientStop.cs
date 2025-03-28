namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public struct GradientStop : IEquatable<GradientStop>
{
    public double Offset { get; }
    public Color Color { get; }

    public GradientStop(Color color, double offset)
    {
        Color = color;
        Offset = offset;
    }

    public bool Equals(GradientStop other)
    {
        return Offset.Equals(other.Offset) && Color.Equals(other.Color);
    }

    public override bool Equals(object? obj)
    {
        return obj is GradientStop other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Offset, Color);
    }
}

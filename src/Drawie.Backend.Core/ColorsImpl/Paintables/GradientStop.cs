namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public struct GradientStop
{
    public double Offset { get; }
    public Color Color { get; }

    public GradientStop(Color color, double offset)
    {
        Color = color;
        Offset = offset;
    }
}

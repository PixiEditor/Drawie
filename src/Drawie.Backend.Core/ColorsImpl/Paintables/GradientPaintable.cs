using Drawie.Backend.Core.Shaders;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public abstract class GradientPaintable : Paintable
{
    public override bool AnythingVisible => GradientStops is { Count: > 0 } && GradientStops.Any(x => x.Color.A > 0);
    public List<GradientStop> GradientStops { get; }

    protected Shader lastShader;

    public GradientPaintable(IEnumerable<GradientStop> gradientStops)
    {
        GradientStops = new List<GradientStop>(gradientStops);
    }

    internal override Shader? GetShaderCached()
    {
        return lastShader;
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
        lastShader?.Dispose();
        lastShader = null;
    }

    public override void Dispose()
    {
        lastShader?.Dispose();
    }
}

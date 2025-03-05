using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class RadialGradientPaintable : GradientPaintable
{
    public VecD Center { get; set; }
    public double Radius { get; set; }

    private Color[] lastColors;
    private float[] lastOffsets;
    private VecD lastCenter;
    private double lastRadius;
    private Matrix3X3 lastLocalMatrix;
    private RectD lastBounds;

    private Shader? lastShader;

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
        if (
            lastShader != null
            && lastCenter == Center
            && lastRadius == Radius
            && lastColors != null
            && lastColors.SequenceEqual(colors) && lastOffsets != null
            && lastOffsets.SequenceEqual(offsets)
            && lastLocalMatrix == matrix
            && lastBounds == bounds)
        {
            return lastShader;
        }

        lastShader?.Dispose();
        lastShader = null;
        lastCenter = Center;
        lastRadius = Radius;
        lastColors = colors;
        lastOffsets = offsets;
        lastLocalMatrix = matrix;
        lastBounds = bounds;

        VecD center = new VecD(Center.X * bounds.Width + bounds.X, Center.Y * bounds.Height + bounds.Y);
        double radius = Radius * bounds.Width;
        lastShader = Shader.CreateRadialGradient(center, (float)radius, colors, offsets, matrix);
        return lastShader;
    }
}

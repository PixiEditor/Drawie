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
    private Matrix3X3? lastTransform;

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
            && lastBounds == bounds
            && lastTransform == Transform)
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
        lastTransform = Transform;

        Matrix3X3 finalMatrix = matrix;
        if (Transform != null)
        {
            finalMatrix = matrix.Concat(Transform.Value);
        }

        VecD center = AbsoluteValues
            ? Center
            : new VecD(Center.X * bounds.Width + bounds.X, Center.Y * bounds.Height + bounds.Y);
        double radius = AbsoluteValues ? Radius : Radius * bounds.Width;
        lastShader = Shader.CreateRadialGradient(center, (float)radius, colors, offsets, finalMatrix);
        return lastShader;
    }

    public override Paintable? Clone()
    {
        return new RadialGradientPaintable(Center, Radius, GradientStops.Select(x => x));
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

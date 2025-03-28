using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class LinearGradientPaintable : GradientPaintable, IStartEndPaintable
{
    public VecD Start { get; set; }
    public VecD End { get; set; }

    private VecD lastStart;
    private VecD lastEnd;
    private Color[] lastColors;
    private double[] lastOffsets;
    private Matrix3X3 lastMatrix;
    private RectD lastBounds;
    private Matrix3X3? lastTransform;

    public LinearGradientPaintable(VecD start, VecD end, IEnumerable<GradientStop> gradientStops) : base(gradientStops)
    {
        Start = start;
        End = end;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        if (EqualsLast(default, bounds, out var colors, out var offsets, out var shader))
        {
            return shader;
        }

        UpdateLast(default, bounds, colors, offsets);

        Matrix3X3 finalMatrix = matrix;

        if (Transform != null)
        {
            finalMatrix = matrix.Concat(Transform.Value);
        }

        VecD start = AbsoluteValues
            ? Start
            : new VecD(Start.X * bounds.Width + bounds.X, Start.Y * bounds.Height + bounds.Y);
        VecD end = AbsoluteValues ? End : new VecD(End.X * bounds.Width + bounds.X, End.Y * bounds.Height + bounds.Y);
        lastShader = Shader.CreateLinearGradient(start, end,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray(),
            finalMatrix);

        return lastShader;
    }

    public override Paintable? Clone()
    {
        return new LinearGradientPaintable(Start, End, GradientStops.Select(x => x).ToList());
    }

    private void UpdateLast(Matrix3X3 matrix, RectD bounds, Color[] colors, double[] offsets)
    {
        lastShader?.Dispose();
        lastShader = null;
        lastStart = Start;
        lastEnd = End;
        lastColors = colors;
        lastOffsets = offsets;
        lastMatrix = matrix;
        lastBounds = bounds;
        lastTransform = Transform;
    }

    private bool EqualsLast(Matrix3X3 matrix, RectD bounds, out Color[] colors, out double[] offsets,
        out Shader? shader)
    {
        colors = GradientStops.Select(x => x.Color).ToArray();
        offsets = GradientStops.Select(x => x.Offset).ToArray();
        if (lastShader != null && lastStart == Start && lastEnd == End && lastColors != null &&
            lastColors.SequenceEqual(colors) &&
            lastOffsets != null && lastOffsets.SequenceEqual(offsets) && lastMatrix == matrix
            && lastBounds == bounds && lastTransform == Transform)
        {
            shader = lastShader;
            return true;
        }

        shader = null;
        return false;
    }

    public void TempUpdateWithStartEnd(VecD start, VecD end)
    {
        lastShader?.Dispose();
        lastShader = null;

        lastShader = Shader.CreateLinearGradient(start, end,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray());
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

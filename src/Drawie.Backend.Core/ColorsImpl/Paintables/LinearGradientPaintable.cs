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
    private Shader lastShader;
    private Matrix3X3 lastMatrix;
    private RectD lastBounds;

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

        VecD start = new VecD(Start.X * bounds.Width + bounds.X, Start.Y * bounds.Height + bounds.Y);
        VecD end = new VecD(End.X * bounds.Width + bounds.X, End.Y * bounds.Height + bounds.Y);
        lastShader = Shader.CreateLinearGradient(start, end,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray(),
            matrix);

        return lastShader;
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
    }

    private bool EqualsLast(Matrix3X3 matrix, RectD bounds, out Color[] colors, out double[] offsets,
        out Shader? shader)
    {
        colors = GradientStops.Select(x => x.Color).ToArray();
        offsets = GradientStops.Select(x => x.Offset).ToArray();
        if (lastShader != null && lastStart == Start && lastEnd == End && lastColors != null &&
            lastColors.SequenceEqual(colors) &&
            lastOffsets != null && lastOffsets.SequenceEqual(offsets) && lastMatrix == matrix && lastBounds == bounds)
        {
            shader = lastShader;
            return true;
        }

        shader = null;
        return false;
    }

    public void UpdateWithStartEnd(VecD start, VecD end)
    {
        lastShader?.Dispose();
        lastShader = null;

        lastShader = Shader.CreateLinearGradient(start, end,
            GradientStops.Select(x => x.Color).ToArray(),
            GradientStops.Select(x => (float)x.Offset).ToArray());
    }
}

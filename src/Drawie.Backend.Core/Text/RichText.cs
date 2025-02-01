using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class RichText
{
    public const double PtToPx = 1.3333333333333333;
    public string RawText { get; set; }

    public string FormattedText { get; }

    public string[] Lines { get; }
    public bool Fill { get; set; }
    public Color FillColor { get; set; }
    public float StrokeWidth { get; set; }
    public Color StrokeColor { get; set; }
    public double MaxWidth { get; set; } = double.MaxValue;
    public double? Spacing { get; set; }

    public RichText(string text, double maxWidth = double.MaxValue)
    {
        if (text == null)
        {
            text = string.Empty;
        }

        RawText = text;
        MaxWidth = maxWidth;

        FormattedText = text.Replace('\n', ' ');
        Lines = text.Split('\n');
    }

    public void Paint(Canvas canvas, VecD position, Font font, Paint paint, VectorPath? onPath)
    {
        if (onPath != null)
        {
            canvas.DrawTextOnPath(onPath, FormattedText, position, font, paint);
        }
        else
        {
            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];

                VecD linePosition = position + GetLineOffset(i, font);
                bool hasStroke = StrokeWidth > 0;
                bool hasFill = Fill && FillColor.A > 0;
                bool strokeAndFillEqual = StrokeColor == FillColor;

                if (hasStroke && hasFill && strokeAndFillEqual)
                {
                    paint.Style = PaintStyle.StrokeAndFill;
                    paint.Color = StrokeColor;
                    paint.StrokeWidth = StrokeWidth;

                    PaintLine(canvas, line, linePosition, font, paint);
                }
                else
                {
                    if (hasStroke)
                    {
                        paint.Style = PaintStyle.Stroke;
                        paint.Color = StrokeColor;
                        paint.StrokeWidth = StrokeWidth;
                        PaintLine(canvas, line, linePosition, font, paint);
                    }

                    if (hasFill)
                    {
                        paint.Style = PaintStyle.Fill;
                        paint.Color = FillColor;
                        PaintLine(canvas, line, linePosition, font, paint);
                    }
                }
            }
        }
    }

    private void PaintLine(Canvas canvas, string line, VecD position, Font font, Paint paint)
    {
        canvas.DrawText(line, position, font, paint);
    }

    public RectD MeasureBounds(Font font)
    {
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        RectD? finalBounds = null;
        double height = 0;

        for (var i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            font.MeasureText(line, out RectD bounds, measurementPaint);

            if (finalBounds == null)
            {
                finalBounds = bounds;
            }
            else
            {
                finalBounds = finalBounds.Value.Union(bounds);
            }

            if (Lines.Length == 1)
            {
                height = bounds.Height;
            }
        }

        if (Lines.Length > 1)
        {
            height = GetLineOffset(Lines.Length, font).Y;
        }

        if (finalBounds == null)
        {
            return RectD.Empty;
        }

        return new RectD(finalBounds.Value.X, finalBounds.Value.Y, finalBounds.Value.Width, height);
    }

    public VecF[] GetGlyphPositions(Font font)
    {
        var glyphPositions = new VecF[RawText.Replace("\n", string.Empty).Length + Lines.Length];
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        int startingIndex = 0;
        for (int i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            VecD lineOffset = GetLineOffset(i, font);
            VecF[] lineGlyphPositions = font.GetGlyphPositions(line);
            for (int j = 0; j < line.Length; j++)
            {
                glyphPositions[startingIndex + j] = lineGlyphPositions[j] + lineOffset;
            }

            if (line.Length == 0)
            {
                glyphPositions[startingIndex] = new VecF(0, (float)lineOffset.Y);
                startingIndex++;
                continue;
            }

            float lastGlyphWidth = font.GetGlyphWidths(line[^1].ToString(), measurementPaint).FirstOrDefault();
            glyphPositions[startingIndex + line.Length] =
                new VecF(glyphPositions[startingIndex + line.Length - 1].X + lastGlyphWidth, (float)lineOffset.Y);

            startingIndex += line.Length + 1;
        }

        return glyphPositions;
    }

    public float[] GetGlyphWidths(Font font)
    {
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        var glyphWidths = new float[RawText.Replace("\n", string.Empty).Length];
        for (int i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            float[] lineGlyphWidths = font.GetGlyphWidths(line, measurementPaint);
            for (int j = 0; j < line.Length; j++)
            {
                if (j + i >= glyphWidths.Length)
                {
                    break;
                }

                if (lineGlyphWidths.Length <= j)
                {
                    break;
                }

                glyphWidths[i + j] = lineGlyphWidths[j];
            }
        }

        return glyphWidths;
    }

    private VecD GetLineOffset(int lineIndex, Font font)
    {
        double lineHeight = Spacing ?? font.Size * PtToPx;
        return new VecD(0, lineIndex * lineHeight);
    }

    public int IndexOnLine(int cursorPosition, out int lineIndex)
    {
        int index = 0;
        lineIndex = 0;
        for (int i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            if (cursorPosition <= index + line.Length)
            {
                lineIndex = i;
                return cursorPosition - index;
            }

            index += line.Length + 1;
        }

        return cursorPosition;
    }

    public int GetIndexOnLine(int line, int index)
    {
        int currentIndex = 0;
        int lineZeroIndex = 0;
        for (int i = 0; i <= line; i++)
        {
            lineZeroIndex = currentIndex;
            currentIndex += Lines[i].Length + 1;
        }

        return Math.Clamp(lineZeroIndex + index, lineZeroIndex, lineZeroIndex + Lines[line].Length);
    }

    public (int lineStart, int lineEnd) GetLineStartEnd(int lineIndex)
    {
        int currentIndex = 0;
        for (int i = 0; i < lineIndex; i++)
        {
            currentIndex += Lines[i].Length + 1;
        }

        return (currentIndex, currentIndex + Lines[lineIndex].Length + 1);
    }
}

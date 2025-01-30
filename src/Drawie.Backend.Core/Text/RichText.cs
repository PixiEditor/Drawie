using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class RichText
{
    public string RawText { get; set; }

    public string FormattedText { get; }

    public string[] Lines { get; }
    public bool Fill { get; set; }
    public Color FillColor { get; set; }
    public float StrokeWidth { get; set; }
    public Color StrokeColor { get; set; }
    public double MaxWidth { get; set; } = double.MaxValue;

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
            VecD linePosition = position;
            foreach (var line in Lines)
            {
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

                paint.Style = PaintStyle.StrokeAndFill; // for measurements
                paint.StrokeWidth = 0;

                if (string.IsNullOrEmpty(line))
                {
                    linePosition.Y += font.Size;
                    continue;
                }

                font.MeasureText(line, out RectD bounds, paint);

                linePosition.Y += bounds.Height;
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

        double height = 0;
        double width = 0;
        double y = 0;
        double x = 0;

        foreach (var line in Lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                height += font.Size;
                continue;
            }

            font.MeasureText(line, out RectD bounds, measurementPaint);

            if (bounds.Width > width)
            {
                width = bounds.Width;
            }

            if (bounds.Y < y)
            {
                y = bounds.Y;
            }

            if (bounds.X < x)
            {
                x = bounds.X;
            }

            height += bounds.Height;
        }

        return new RectD(x, y, width, height);
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
            VecD lineOffset = GetLineOffset(i, font, measurementPaint);
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
                glyphWidths[i + j] = lineGlyphWidths[j];
            }
        }

        return glyphWidths;
    }

    private VecD GetLineOffset(int lineIndex, Font font, Paint measurementPaint)
    {
        double y = 0;
        for (int i = 0; i < lineIndex; i++)
        {
            if (string.IsNullOrEmpty(Lines[i]))
            {
                y += font.Size;
            }
            else
            {
                font.MeasureText(Lines[i], out RectD bounds, measurementPaint);
                y += bounds.Height;
            }
        }

        return new VecD(0, y);
    }
}

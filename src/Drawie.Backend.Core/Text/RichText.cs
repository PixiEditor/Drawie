using System.Globalization;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
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
    public Paintable FillPaintable { get; set; }
    public float StrokeWidth { get; set; }
    public Paintable StrokePaintable { get; set; }
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

    public void Paint(Canvas canvas, VecD position, Font font, Paint paint, VectorPath? onPath, VecD? pathOffset = null)
    {
        if (pathOffset == null)
        {
            pathOffset = VecD.Zero;
        }

        bool hasStroke = StrokeWidth > 0;
        bool hasFill = Fill && FillPaintable.AnythingVisible;
        bool strokeAndFillEqual = StrokePaintable == FillPaintable;

        if (onPath != null)
        {
            if (hasStroke && hasFill && strokeAndFillEqual)
            {
                paint.Style = PaintStyle.StrokeAndFill;
                paint.SetPaintable(StrokePaintable);
                paint.StrokeWidth = StrokeWidth;

                canvas.DrawTextOnPath(onPath, FormattedText, pathOffset.Value, font, paint);
            }
            else
            {
                if (hasStroke)
                {
                    paint.Style = PaintStyle.Stroke;
                    paint.SetPaintable(StrokePaintable);
                    paint.StrokeWidth = StrokeWidth;
                    canvas.DrawTextOnPath(onPath, FormattedText, pathOffset.Value, font, paint);
                }

                if (hasFill)
                {
                    paint.Style = PaintStyle.Fill;
                    paint.SetPaintable(FillPaintable);
                    canvas.DrawTextOnPath(onPath, FormattedText, pathOffset.Value, font, paint);
                }
            }
        }
        else
        {
            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];

                VecD linePosition = position + GetLineOffset(i, font);

                if (hasStroke && hasFill && strokeAndFillEqual)
                {
                    paint.Style = PaintStyle.StrokeAndFill;
                    paint.SetPaintable(StrokePaintable);
                    paint.StrokeWidth = StrokeWidth;

                    PaintLine(canvas, line, linePosition, font, paint);
                }
                else
                {
                    if (hasStroke)
                    {
                        paint.Style = PaintStyle.Stroke;
                        paint.SetPaintable(StrokePaintable);
                        paint.StrokeWidth = StrokeWidth;
                        PaintLine(canvas, line, linePosition, font, paint);
                    }

                    if (hasFill)
                    {
                        paint.Style = PaintStyle.Fill;
                        paint.SetPaintable(FillPaintable);
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
        if (font == null)
        {
            return RectD.Empty;
        }

        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        RectD? finalBounds = null;
        double height = 0;
        RectD? lastBounds = null;

        for (var i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            font.MeasureText(line, out RectD bounds, measurementPaint);

            lastBounds = bounds;

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

        if (Lines.Length > 1 && lastBounds != null)
        {
            height = GetLineOffset(Lines.Length - 1, font).Y + lastBounds.Value.Height;
        }

        if (finalBounds == null)
        {
            return RectD.Empty;
        }

        return new RectD(finalBounds.Value.X, finalBounds.Value.Y, finalBounds.Value.Width, height);
    }

    public VecF[] GetGlyphPositions(Font font)
    {
        if (Lines == null || RawText == null || Lines.Length == 0 || font == null)
        {
            return [];
        }

        var elements = StringInfo.GetTextElementEnumerator(RawText.Replace("\n", string.Empty));
        int count = 0;
        while (elements.MoveNext())
        {
            count++;
        }

        var glyphPositions = new VecF[count + Lines.Length];
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        int startingIndex = 0;
        for (int i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            VecD lineOffset = GetLineOffset(i, font);
            VecF[] lineGlyphPositions = font.GetGlyphPositions(line);
            for (int j = 0; j < lineGlyphPositions.Length; j++)
            {
                glyphPositions[startingIndex + j] = lineGlyphPositions[j] + lineOffset;
            }

            if (line.Length == 0)
            {
                glyphPositions[startingIndex] = new VecF(0, (float)lineOffset.Y);
                startingIndex++;
                continue;
            }

            var actualLineLength = GetActualLineLength(line);

            var lineElements = StringInfo.GetTextElementEnumerator(line);
            string lastElement = null;
            while(lineElements.MoveNext())
            {
                lastElement = lineElements.GetTextElement();
            }

            float lastGlyphWidth = font.GetGlyphWidths(lastElement, measurementPaint).FirstOrDefault();
            glyphPositions[startingIndex + actualLineLength] =
                new VecF(glyphPositions[startingIndex + actualLineLength - 1].X + lastGlyphWidth, (float)lineOffset.Y);

            startingIndex += actualLineLength + 1;
        }

        return glyphPositions;
    }

    public float[] GetGlyphWidths(Font font)
    {
        if (font == null)
        {
            return [];
        }

        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        var elements = StringInfo.GetTextElementEnumerator(RawText.Replace("\n", string.Empty));
        int count = 0;
        while (elements.MoveNext())
        {
            count++;
        }

        var glyphWidths = new float[count + Lines.Length];
        int startingIndex = 0;
        for (int i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            float[] lineGlyphWidths = font.GetGlyphWidths(line, measurementPaint);
            for (int j = 0; j < lineGlyphWidths.Length; j++)
            {
                glyphWidths[startingIndex + j] = lineGlyphWidths[j];
            }

            if (line.Length == 0)
            {
                glyphWidths[startingIndex] = 0;
                startingIndex++;
                continue;
            }

            var actualLineLength = GetActualLineLength(line);

            startingIndex += actualLineLength + 1;
        }

        return glyphWidths;
    }

    private static int GetActualLineLength(string line)
    {
        int actualLineLength = 0;
        for (int j = 0; j < line.Length; j++)
        {
            if (char.IsHighSurrogate(line[j]) && j + 1 < line.Length && char.IsLowSurrogate(line[j + 1]))
            {
                actualLineLength++;
                j++;
            }
            else
            {
                actualLineLength++;
            }
        }

        return actualLineLength;
    }

    public VecD GetLineOffset(int lineIndex, Font font)
    {
        if (font == null)
        {
            return VecD.Zero;
        }

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

    public VectorPath ToPath(Font font)
    {
        VectorPath path = new VectorPath();

        for (var i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            Matrix3X3 matrix = Matrix3X3.CreateTranslation(0, (float)GetLineOffset(i, font).Y);
            path.AddPath(font.GetTextPath(line), matrix, AddPathMode.Append);
        }

        return path;
    }
}

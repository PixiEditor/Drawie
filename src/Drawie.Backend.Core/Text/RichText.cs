using System.Globalization;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class RichText : ICacheable
{
    public const double PtToPx = 1.3333333333333333;
    public IReadOnlyList<TextInline> Inlines => InlinesMutable;
    public string RawText => string.Concat(Inlines.Select(x => x.Text));
    public string FormattedText => RawText.Replace('\n', ' ');
    public IReadOnlyCollection<TextInline>[] Lines => ChopInlinesIntoLines();

    public bool Fill { get; set; }
    public Paintable FillPaintable { get; set; }
    public float StrokeWidth { get; set; }
    public Paintable StrokePaintable { get; set; }
    public double MaxWidth { get; set; } = double.MaxValue;
    public double? Spacing { get; set; }

    private List<TextInline> InlinesMutable { get; } = new();

    public int TextGlyphCount
    {
        get
        {
            // TODO: Caching
            int count = 0;
            var iterator = StringInfo.GetTextElementEnumerator(RawText);
            while (iterator.MoveNext())
            {
                count++;
            }

            return count;
        }
    }

    private IReadOnlyCollection<TextInline>[] ChopInlinesIntoLines()
    {
        List<IReadOnlyCollection<TextInline>> lines = new();
        List<TextInline> currentLine = new();
        foreach (TextInline inline in Inlines)
        {
            string[] inlineLines = inline.Text.Split('\n');
            for (int i = 0; i < inlineLines.Length; i++)
            {
                string lineText = inlineLines[i];
                if (!string.IsNullOrEmpty(lineText))
                {
                    var clonedInline = inline.Clone();
                    clonedInline.Text = lineText;
                    currentLine.Add(clonedInline);
                }

                if (i < inlineLines.Length - 1)
                {
                    lines.Add(currentLine);
                    currentLine = new List<TextInline>();
                }
            }
        }

        if (currentLine.Count > 0) lines.Add(currentLine);
        return lines.ToArray();
    }

    public RichText() { }

    public RichText(string text, FontData font, double maxWidth = double.MaxValue)
    {
        MaxWidth = maxWidth;
        InlinesMutable.Add(new TextInline(text ?? string.Empty, font));
    }

    public RichText(IEnumerable<TextInline> inlines, double maxWidth = double.MaxValue)
    {
        MaxWidth = maxWidth;
        InlinesMutable.AddRange(inlines);
    }

    public int IndexOfInline(TextInline inline)
    {
        return InlinesMutable.IndexOf(inline);
    }

    public void UpdateInline(int index, TextInline inline) { InlinesMutable[index] = inline; }
    public void AddInline(TextInline inline) { InlinesMutable.Add(inline); }
    public void AddInline(string text, FontData font) { InlinesMutable.Add(new TextInline(text, font)); }
    public void Clear() { InlinesMutable.Clear(); }

    public TextInline? GetInlineAt(int index, out int inlineStart, out int inlineEnd)
    {
        int offset = 0;
        foreach (TextInline inline in Inlines)
        {
            int end = offset + inline.Text.Length;
            if (index >= offset && index <= end)
            {
                inlineStart = offset;
                inlineEnd = end;
                return inline;
            }

            offset = end;
        }

        inlineStart = -1;
        inlineEnd = -1;
        return null;
    }

    public int GetInlineStart(TextInline inline)
    {
        int offset = 0;
        foreach (TextInline current in Inlines)
        {
            if (ReferenceEquals(current, inline)) return offset;
            offset += current.Text.Length;
        }

        return -1;
    }

    public int GetInlineEnd(TextInline inline)
    {
        int start = GetInlineStart(inline);
        return start < 0 ? -1 : start + inline.Text.Length;
    }

    public void Paint(Canvas canvas, VecD position, Paint paint, VectorPath? onPath = null, VecD? pathOffset = null)
    {
        if (pathOffset == null) pathOffset = VecD.Zero;
        bool hasStroke = StrokeWidth > 0;
        bool hasFill = Fill && FillPaintable.AnythingVisible;
        bool strokeAndFillEqual = StrokePaintable == FillPaintable;
        if (onPath != null)
        {
            PaintOnPath(canvas, position, paint, onPath, pathOffset.Value);
            return;
        }

        double x = position.X;
        double y = position.Y;
        foreach (var line in Lines)
        {
            double lineHeight = 0;
            double lineX = x;
            foreach (TextInline inline in line)
            {
                if (string.IsNullOrEmpty(inline.Text)) continue;
                using Font font = inline.Font.ToFont();
                lineHeight = Math.Max(lineHeight, inline.LineHeight > 0 ? inline.LineHeight : font.Size * PtToPx);
                VecD inlinePosition = new VecD(lineX, y);
                if (hasStroke && hasFill && strokeAndFillEqual)
                {
                    paint.Style = PaintStyle.StrokeAndFill;
                    paint.SetPaintable(StrokePaintable);
                    paint.StrokeWidth = StrokeWidth;
                    canvas.DrawText(inline.Text, inlinePosition, font, paint);
                }
                else
                {
                    if (hasStroke)
                    {
                        paint.Style = PaintStyle.Stroke;
                        paint.SetPaintable(StrokePaintable);
                        paint.StrokeWidth = StrokeWidth;
                        canvas.DrawText(inline.Text, inlinePosition, font, paint);
                    }

                    if (hasFill)
                    {
                        paint.Style = PaintStyle.Fill;
                        paint.SetPaintable(FillPaintable);
                        canvas.DrawText(inline.Text, inlinePosition, font, paint);
                    }
                }

                lineX += font.MeasureText(inline.Text);
            }

            y += lineHeight;
        }
    }

    private void PaintOnPath(Canvas canvas, VecD position, Paint paint, VectorPath path, VecD pathOffset)
    {
        bool hasStroke = StrokeWidth > 0;
        bool hasFill = Fill && FillPaintable.AnythingVisible;
        bool strokeAndFillEqual = StrokePaintable == FillPaintable;
        foreach (TextInline inline in Inlines)
        {
            if (string.IsNullOrEmpty(inline.Text)) continue;
            using Font font = inline.Font.ToFont();
            if (hasStroke && hasFill && strokeAndFillEqual)
            {
                paint.Style = PaintStyle.StrokeAndFill;
                paint.SetPaintable(StrokePaintable);
                paint.StrokeWidth = StrokeWidth;
                canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
            }
            else
            {
                if (hasStroke)
                {
                    paint.Style = PaintStyle.Stroke;
                    paint.SetPaintable(StrokePaintable);
                    paint.StrokeWidth = StrokeWidth;
                    canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
                }

                if (hasFill)
                {
                    paint.Style = PaintStyle.Fill;
                    paint.SetPaintable(FillPaintable);
                    canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
                }
            }

            pathOffset += new VecD(font.MeasureText(inline.Text), 0);
        }
    }

    public RectD MeasureBounds()
    {
        if (Inlines.Count == 0) return RectD.Empty;
        RectD? bounds = null;
        double x = 0;
        double y = 0;
        double lineHeight = 0;
        foreach (TextInline inline in Inlines)
        {
            if (string.IsNullOrEmpty(inline.Text)) continue;
            using Font font = inline.Font.ToFont();
            string[] lines = inline.Text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!string.IsNullOrEmpty(line))
                {
                    font.MeasureText(line, out RectD inlineBounds);
                    inlineBounds = new RectD(inlineBounds.X + x, inlineBounds.Y + y, inlineBounds.Width,
                        inlineBounds.Height);
                    bounds = bounds == null ? inlineBounds : bounds.Value.Union(inlineBounds);
                }

                if (i < lines.Length - 1)
                {
                    y += lineHeight > 0 ? lineHeight : font.Size * PtToPx;
                    x = 0;
                    lineHeight = 0;
                }
            }

            if (lines.Length > 0)
            {
                string lastLine = lines[^1];
                lineHeight = Math.Max(lineHeight, inline.LineHeight > 0 ? inline.LineHeight : font.Size * PtToPx);
                if (!string.IsNullOrEmpty(lastLine)) x += font.MeasureText(lastLine);
            }
        }

        return bounds ?? RectD.Empty;
    }

    public VecF[] GetGlyphPositions()
    {
        if (Inlines.Count == 0) return [];
        List<VecF> positions = new();
        double x = 0;
        double y = 0;
        foreach (TextInline inline in Inlines)
        {
            if (string.IsNullOrEmpty(inline.Text)) continue;
            using Font font = inline.Font.ToFont();
            foreach (string line in inline.Text.Split('\n'))
            {
                VecF[] linePositions = font.GetGlyphPositions(line);
                foreach (VecF glyphPosition in linePositions)
                {
                    positions.Add(glyphPosition + new VecF((float)x, (float)y));
                }

                if (!string.IsNullOrEmpty(line)) x += font.MeasureText(line);
                y += inline.LineHeight > 0 ? inline.LineHeight : font.Size * PtToPx;
                x = 0;
            }
        }

        return positions.ToArray();
    }

    public float[] GetGlyphWidths()
    {
        if (Inlines.Count == 0) return [];
        List<float> widths = new();
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;
        foreach (TextInline inline in Inlines)
        {
            if (string.IsNullOrEmpty(inline.Text)) continue;
            using Font font = inline.Font.ToFont();
            foreach (string line in inline.Text.Split('\n'))
            {
                widths.AddRange(font.GetGlyphWidths(line, measurementPaint));
            }
        }

        return widths.ToArray();
    }

    public int IndexOnLine(int cursorPosition, out int lineIndex)
    {
        int index = 0;
        lineIndex = 0;
        foreach (var line in Lines)
        {
            var lineLength = GetLineLength(line);
            if (cursorPosition <= index + lineLength) return cursorPosition - index;
            index += lineLength + 1;
            lineIndex++;
        }

        return cursorPosition - index;
    }

    private int GetLineLength(IReadOnlyCollection<TextInline> line)
    {
        int length = 0;
        foreach (var inline in line)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(inline.Text);
            while (enumerator.MoveNext()) length++;
        }

        return length;
    }

    public int GetIndexOnLine(int line, int index)
    {
        int currentIndex = 0;
        for (int i = 0; i < line; i++) currentIndex += GetLineLength(Lines[i]) + 1;
        return Math.Clamp(currentIndex + index, currentIndex, currentIndex + GetLineLength(Lines[line]));
    }

    public (int lineStart, int lineEnd) GetLineStartEnd(int lineIndex)
    {
        int currentIndex = 0;
        for (int i = 0; i < lineIndex; i++) currentIndex += GetLineLength(Lines[i]) + 1;
        return (currentIndex, currentIndex + GetLineLength(Lines[lineIndex]) + 1);
    }

    public VectorPath ToPath()
    {
        VectorPath path = new VectorPath();
        double x = 0;
        double y = 0;
        foreach (TextInline inline in Inlines)
        {
            using Font font = inline.Font.ToFont();
            foreach (string line in inline.Text.Split('\n'))
            {
                Matrix3X3 matrix = Matrix3X3.CreateTranslation((float)x, (float)y);
                path.AddPath(font.GetTextPath(line), matrix, AddPathMode.Append);
                x += font.MeasureText(line);
                if (line != inline.Text.Split('\n').Last())
                {
                    x = 0;
                    y += inline.LineHeight > 0 ? inline.LineHeight : font.Size * PtToPx;
                }
            }
        }

        return path;
    }

    public override string ToString()
    {
        return FormattedText;
    }

    public int GetCacheHash()
    {
        HashCode hash = new HashCode();
        hash.Add(MaxWidth);
        hash.Add(Spacing);
        foreach (TextInline inline in Inlines)
        {
            hash.Add(inline.GetCacheHash());
        }

        return hash.ToHashCode();
    }

    public VecD GetLineOffset(int lineIndex)
    {
        if (lineIndex <= 0)
            return VecD.Zero;

        double y = 0;

        for (int i = 0; i < lineIndex; i++)
            y += GetLineHeight(i);

        return new VecD(0, y);
    }

    // TODO: Calculate once and cache the line heights for performance
    public double GetLineHeight(int lineIndex)
    {
        int currentLine = 0;
        double lineHeight = 0;

        foreach (TextInline inline in Inlines)
        {
            string[] lines = inline.Text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (currentLine == lineIndex)
                {
                    double inlineLineHeight = inline.LineHeight > 0
                        ? inline.LineHeight
                        : inline.Font.Size * PtToPx;

                    lineHeight = Math.Max(lineHeight, inlineLineHeight);
                }

                if (i < lines.Length - 1)
                {
                    if (currentLine == lineIndex)
                        return lineHeight;

                    currentLine++;
                }
            }

            if (lines.Length > 0 && currentLine == lineIndex)
            {
                double inlineLineHeight = inline.LineHeight > 0
                    ? inline.LineHeight
                    : inline.Font.Size * PtToPx;

                lineHeight = Math.Max(lineHeight, inlineLineHeight);
            }
        }

        return lineHeight;
    }

    public RichText Clone()
    {
        var clonedInlines = Inlines.Select(inline => inline.Clone()).ToList();

        return new RichText(clonedInlines, MaxWidth)
        {
            Fill = Fill,
            FillPaintable = FillPaintable,
            StrokeWidth = StrokeWidth,
            StrokePaintable = StrokePaintable,
            Spacing = Spacing,
        };
    }
}

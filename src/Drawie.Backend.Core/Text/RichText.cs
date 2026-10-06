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
    public double MaxWidth { get; set; } = double.MaxValue;
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
            if (string.IsNullOrEmpty(inline.Text))
            {
                continue;
            }

            var inlineLines = inline.Text.Split('\n');

            for (int i = 0; i < inlineLines.Length; i++)
            {
                string lineText = inlineLines[i];
                if (!string.IsNullOrEmpty(lineText))
                {
                    var clonedInline = inline.Clone();
                    clonedInline.Text = lineText;
                    currentLine.Add(clonedInline);
                }
                else
                {
                    currentLine.Add(new TextInline("\n", inline.Font));
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

    private List<string> Split(string inlineText)
    {
        List<string> splits = new();
        string currentSplit = string.Empty;
        foreach (var aChar in inlineText)
        {
            if (aChar == '\n')
            {
                splits.Add(currentSplit);
                splits.Add("\n");
                currentSplit = string.Empty;
            }
            else
            {
                currentSplit += aChar;
            }
        }

        splits.Add(currentSplit);
        return splits;
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

    public TextInline? GetInlineAt(int cursorPos, out int inlineStart, out int inlineEnd)
    {
        int offset = 0;

        foreach (var inline in InlinesMutable)
        {
            int length = GetTextElementLength(inline.Text);

            if (cursorPos >= offset && cursorPos <= offset + length)
            {
                inlineStart = offset;
                inlineEnd = offset + length;
                return inline;
            }

            offset += length;
        }

        inlineStart = -1;
        inlineEnd = -1;
        return null;
    }

    private static int GetTextElementLength(string text)
    {
        int length = 0;
        var enumerator = StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext())
            length++;

        return length;
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
        if (pathOffset == null)
            pathOffset = VecD.Zero;

        if (onPath != null)
        {
            PaintOnPath(canvas, position, paint, onPath, pathOffset.Value);
            return;
        }

        double x = position.X;
        double y = position.Y;
        double boundingWidth = MeasureBounds().Width;

        for (var index = 0; index < Lines.Length; index++)
        {
            var line = Lines[index];
            double maxLineHeight = 0;
            double maxFontSize = 0;
            double measuredLineWidth = 0;

            bool allEmpty = true;
            foreach (TextInline inline in line)
            {
                bool isEmpty = string.IsNullOrEmpty(inline.Text) || inline.Text == "\n";
                allEmpty &= isEmpty;
                if (isEmpty) continue;
                if (index > 0)
                {
                    maxLineHeight = Math.Max(maxLineHeight, inline.LineHeight * PtToPx);
                    maxFontSize = Math.Max(maxFontSize, inline.Font.Size * PtToPx);
                }

                using Font font = inline.Font.ToFont();
                measuredLineWidth += font.MeasureText(inline.Text);
            }

            if (allEmpty)
            {
                maxLineHeight = line.FirstOrDefault()?.LineHeight * PtToPx ?? 0;
            }

            double lineX = x;

            TextAlign? alignment = null;

            int saved = canvas.Save();

            foreach (TextInline inline in line)
            {
                if (string.IsNullOrEmpty(inline.Text) || inline.Text == "\n")
                    continue;

                using Font font = inline.Font.ToFont();
                if (alignment == null)
                {
                    alignment = inline.Alignment;

                    if (alignment == TextAlign.Center)
                    {
                        canvas.Translate((float)((boundingWidth - measuredLineWidth) / 2f), 0);
                    }
                    else if (alignment == TextAlign.Right)
                    {
                        canvas.Translate((float)(boundingWidth - measuredLineWidth), 0);
                    }
                }

                double topOffset = maxLineHeight;
                double measuredInlineWidth = font.MeasureText(inline.Text);

                VecD inlinePosition = new VecD(lineX, y + topOffset);

                bool hasStroke = inline is { StrokeWidth: > 0, StrokePaintable.AnythingVisible: true };

                bool hasFill = inline is { FillPaintable.AnythingVisible: true, Fill: true };

                bool strokeAndFillEqual = inline.StrokePaintable == inline.FillPaintable;


                if (hasStroke && hasFill && strokeAndFillEqual)
                {
                    paint.Style = PaintStyle.StrokeAndFill;
                    paint.SetPaintable(inline.StrokePaintable);
                    paint.StrokeWidth = inline.StrokeWidth;
                    canvas.DrawText(inline.Text, inlinePosition, TextAlign.Left, font, paint);
                }
                else
                {
                    if (hasStroke)
                    {
                        paint.Style = PaintStyle.Stroke;
                        paint.SetPaintable(inline.StrokePaintable);
                        paint.StrokeWidth = inline.StrokeWidth;
                        canvas.DrawText(inline.Text, inlinePosition, TextAlign.Left, font, paint);
                    }

                    if (hasFill)
                    {
                        paint.Style = PaintStyle.Fill;
                        paint.SetPaintable(inline.FillPaintable);
                        canvas.DrawText(inline.Text, inlinePosition, TextAlign.Left, font, paint);
                    }
                }

                lineX += measuredInlineWidth;
            }

            canvas.RestoreToCount(saved);

            y += maxLineHeight;
        }
    }

    public RectD MeasureBounds()
    {
        if (Inlines.Count == 0) return RectD.Empty;

        RectD? bounds = null;
        double x = 0;
        double y = 0;

        for (var index = 0; index < Lines.Length; index++)
        {
            var line = Lines[index];
            double maxLineHeight = 0;

            bool allEmpty = true;
            foreach (TextInline inline in line)
            {
                bool isEmpty = string.IsNullOrEmpty(inline.Text) || inline.Text == "\n";
                allEmpty &= isEmpty;
                if (isEmpty) continue;
                if(index > 0)
                    maxLineHeight = Math.Max(maxLineHeight, inline.LineHeight * PtToPx);
            }

            if (allEmpty)
            {
                maxLineHeight = line.FirstOrDefault()?.LineHeight * PtToPx ?? 0;
            }

            foreach (TextInline inline in line)
            {
                if (string.IsNullOrEmpty(inline.Text) || inline.Text == "\n")
                    continue;

                using Font font = inline.Font.ToFont();

                font.MeasureText(inline.Text, out RectD inlineBounds);
                inlineBounds = new RectD(
                    inlineBounds.X + x,
                    inlineBounds.Y + y + maxLineHeight,
                    inlineBounds.Width,
                    inlineBounds.Height);

                bounds = bounds == null
                    ? inlineBounds
                    : bounds.Value.Union(inlineBounds);

                x += font.MeasureText(inline.Text);
            }

            y += maxLineHeight;
            x = 0;
        }

        return bounds ?? RectD.Empty;
    }

    public VecF[] GetGlyphPositions(bool includeEndPosition = false)
    {
        if (Inlines.Count == 0) return [];

        List<VecF> positions = new();
        double x = 0;
        double y = 0;
        double boundingWidth = MeasureBounds().Width;

        for (var index = 0; index < Lines.Length; index++)
        {
            var line = Lines[index];
            double maxLineHeight = 0;
            TextAlign? alignment = null;
            double measuredLineWidth = 0;

            bool allEmpty = true;
            foreach (TextInline inline in line)
            {
                bool isEmpty = string.IsNullOrEmpty(inline.Text) || inline.Text == "\n";
                allEmpty &= isEmpty;
                if (isEmpty) continue;
                if (index > 0)
                    maxLineHeight = Math.Max(maxLineHeight, inline.LineHeight * PtToPx);
                using Font font = inline.Font.ToFont();
                measuredLineWidth += font.MeasureText(inline.Text);
            }

            if (allEmpty)
            {
                maxLineHeight = line.FirstOrDefault()?.LineHeight * PtToPx ?? 0;
            }

            double offsetX = 0;
            foreach (TextInline inline in line)
            {
                if (string.IsNullOrEmpty(inline.Text) || inline.Text == "\n")
                    continue;

                using Font font = inline.Font.ToFont();
                var measuredLine = font.MeasureText(inline.Text);

                if (alignment == null)
                {
                    alignment = inline.Alignment;
                    if (alignment == TextAlign.Center)
                    {
                        offsetX = (boundingWidth - measuredLineWidth) / 2f;
                    }
                    else if (alignment == TextAlign.Right)
                    {
                        offsetX = boundingWidth - measuredLineWidth;
                    }
                }

                VecF[] glyphPositions = font.GetGlyphPositions(inline.Text);

                foreach (VecF glyphPosition in glyphPositions)
                    positions.Add(glyphPosition + new VecF((float)x + (float)offsetX, (float)y + (float)maxLineHeight));

                x += measuredLine;
            }

            if (includeEndPosition)
                positions.Add(new VecF((float)x + (float)offsetX, (float)y + (float)maxLineHeight));

            y += maxLineHeight;
            x = 0;
        }

        return positions.ToArray();
    }


    private void PaintOnPath(Canvas canvas, VecD position, Paint paint, VectorPath path, VecD pathOffset)
    {
        foreach (TextInline inline in Inlines)
        {
            if (string.IsNullOrEmpty(inline.Text)) continue;
            using Font font = inline.Font.ToFont();
            bool hasStroke = inline.StrokeWidth > 0 && inline.StrokePaintable != null &&
                             inline is { StrokePaintable.AnythingVisible: true };
            bool hasFill = inline.FillPaintable != null &&
                           inline is { Fill: true, FillPaintable.AnythingVisible: true };
            bool strokeAndFillEqual = inline.StrokePaintable == inline.FillPaintable;
            if (hasStroke && hasFill && strokeAndFillEqual)
            {
                paint.Style = PaintStyle.StrokeAndFill;
                paint.SetPaintable(inline.StrokePaintable);
                paint.StrokeWidth = inline.StrokeWidth;
                canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
            }
            else
            {
                if (hasStroke)
                {
                    paint.Style = PaintStyle.Stroke;
                    paint.SetPaintable(inline.StrokePaintable);
                    paint.StrokeWidth = inline.StrokeWidth;
                    canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
                }

                if (hasFill)
                {
                    paint.Style = PaintStyle.Fill;
                    paint.SetPaintable(inline.FillPaintable);
                    canvas.DrawTextOnPath(path, inline.Text, pathOffset, font, paint);
                }
            }

            pathOffset += new VecD(font.MeasureText(inline.Text), 0);
        }
    }

    public List<TextInline> GetInlinesInRange(int from, int to)
    {
        int selectionStart = Math.Min(from, to);
        int selectionEnd = Math.Max(from, to);

        List<TextInline> result = new();

        if (Inlines.Count == 0)
            return result;

        if (selectionStart == selectionEnd)
        {
            var inlineAt = GetInlineAt(selectionStart, out int inlineStart, out int inlineEnd);
            if (inlineAt != null)
            {
                result.Add(inlineAt);
            }

            return result;
        }

        int currentPosition = 0;

        foreach (TextInline inline in Inlines)
        {
            int inlineStart = currentPosition;
            int inlineEnd = currentPosition + inline.Text.Length;

            if (inlineStart < selectionEnd && inlineEnd > selectionStart)
                result.Add(inline);

            currentPosition = inlineEnd;

            if (currentPosition >= selectionEnd)
                break;
        }

        return result;
    }

    public int IndexOnLine(int cursorPosition, out int lineIndex, bool accountNewLines = true)
    {
        for (var i = 0; i < Lines.Length; i++)
        {
            var startEnd = GetLineStartEnd(i, accountNewLines);
            if (cursorPosition >= startEnd.lineStart && cursorPosition <= startEnd.lineEnd)
            {
                lineIndex = i;
                return cursorPosition - startEnd.lineStart;
            }
        }

        lineIndex = Lines.Length - 1;
        return cursorPosition;
    }

    private int GetLineLength(IReadOnlyCollection<TextInline> line, bool newLineIsZeroLength = false)
    {
        int length = 0;
        foreach (var inline in line)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(inline.Text);
            while (enumerator.MoveNext())
            {
                if (enumerator.GetTextElement() == "\n" && newLineIsZeroLength)
                {
                    continue;
                }

                length++;
            }
        }

        return length;
    }

    public int GetIndexOnLine(int line, int index)
    {
        int currentIndex = CountLineLength(line, true);
        return Math.Clamp(currentIndex + index, currentIndex, currentIndex + GetLineLength(Lines[line]));
    }

    public (int lineStart, int lineEnd) GetLineStartEnd(int lineIndex, bool accountNewLines = true)
    {
        var currentIndex = CountLineLength(lineIndex, accountNewLines);
        return (currentIndex, currentIndex + GetLineLength(Lines[lineIndex], true));
    }

    private int CountLineLength(int lineIndex, bool accountNewLines)
    {
        int currentIndex = 0;
        for (int i = 0; i < lineIndex; i++)
        {
            currentIndex += GetLineLength(Lines[i]);
            if (accountNewLines && (Lines[i].Count != 1 || Lines[i].First().Text != "\n"))
            {
                currentIndex++; // Account for the newline character between lines
            }
        }

        return currentIndex;
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
                    y += inline.LineHeight * PtToPx;
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

        if (lineIndex < 0 || lineIndex >= Lines.Length)
            return 0;

        return Lines[lineIndex].Max(inline => inline.LineHeight * PtToPx);
    }

    public RichText Clone()
    {
        var clonedInlines = Inlines.Select(inline => inline.Clone()).ToList();

        return new RichText(clonedInlines, MaxWidth);
    }

    public TextInline SplitInline(int inlineIndex, int cursorPosition, int selectionEnd)
    {
        TextInline inline = InlinesMutable[inlineIndex];

        int inlineStart = GetInlineStart(inline);
        int inlineEnd = GetInlineEnd(inline);

        int start = Math.Clamp(cursorPosition - inlineStart, 0, inlineEnd - inlineStart);
        int end = Math.Clamp(selectionEnd - inlineStart, 0, inlineEnd - inlineStart);

        if (start > end)
            (start, end) = (end, start);

        string[] elements = GetTextElements(inline.Text);

        string before = string.Concat(elements[..start]);
        string selected = string.Concat(elements[start..end]);
        string after = string.Concat(elements[end..]);

        InlinesMutable.RemoveAt(inlineIndex);

        int insertIndex = inlineIndex;

        if (before.Length > 0)
        {
            TextInline beforeInline = inline.Clone();
            beforeInline.Text = before;
            InlinesMutable.Insert(insertIndex++, beforeInline);
        }

        TextInline selectedInline = inline.Clone();
        selectedInline.Text = selected;
        InlinesMutable.Insert(insertIndex++, selectedInline);

        if (after.Length > 0)
        {
            TextInline afterInline = inline.Clone();
            afterInline.Text = after;
            InlinesMutable.Insert(insertIndex, afterInline);
        }

        return selectedInline;
    }

    public TextInline MergeAdjacentInlines(TextInline target)
    {
        int targetIndex = InlinesMutable.IndexOf(target);

        if (targetIndex < 0)
            return target;

        if (targetIndex > 0 &&
            InlinesMutable[targetIndex - 1].HasEqualSettings(InlinesMutable[targetIndex]))
        {
            TextInline previous = InlinesMutable[targetIndex - 1];
            previous.Text += target.Text;
            InlinesMutable.RemoveAt(targetIndex);

            target = previous;
            targetIndex--;
        }

        if (targetIndex + 1 < InlinesMutable.Count &&
            InlinesMutable[targetIndex].HasEqualSettings(InlinesMutable[targetIndex + 1]))
        {
            TextInline next = InlinesMutable[targetIndex + 1];
            target.Text += next.Text;
            InlinesMutable.RemoveAt(targetIndex + 1);
        }

        return target;
    }

    private static string[] GetTextElements(string text)
    {
        var elements = new List<string>();
        var enumerator = StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext())
            elements.Add(enumerator.GetTextElement());

        return elements.ToArray();
    }
}

using System.Globalization;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;

namespace Drawie.Backend.Core.Text;

public class TextInline : ICacheable
{
    public string Text { get; set; }
    public FontData Font { get; set; }
    public TextAlign Alignment { get; set; }
    public float LineHeight { get; set; }
    //public float LetterSpacing { get; set; }
    public bool Fill { get; set; } = true;
    public Paintable? FillPaintable { get; set; }
    public Paintable? StrokePaintable { get; set; }
    public float StrokeWidth { get; set; }

    public int GlyphCount
    {
        get
        {
            var iterator = StringInfo.GetTextElementEnumerator(Text);
            int count = 0;
            while (iterator.MoveNext())
            {
                count++;
            }
            return count;
        }
    }

    //TODO:
    // public TextDecoration Decoration { get; set; }
    // public TextDecorationType DecorationType { get; set; }
    // public float DecorationThicknessPercent { get; set; }
    // public float DecorationOffsetPercent { get; set; }
    // public Paintable DecorationPaintable { get; set; }

    public TextInline(string text, FontData font)
    {
        Text = text;
        Font = font;
        LineHeight = (float)font.Size;
        FillPaintable = Colors.Black;
    }

    public int GetCacheHash()
    {
        HashCode hash = new HashCode();
        hash.Add(Text);
        hash.Add(Font.GetCacheHash());
        hash.Add(LineHeight);
        //hash.Add(LetterSpacing);
        hash.Add(Alignment);
        //hash.Add(Decoration);
        hash.Add(Fill);
        hash.Add(FillPaintable?.GetCacheHash());
        hash.Add(StrokePaintable?.GetCacheHash());
        hash.Add(StrokeWidth);
        return hash.ToHashCode();
    }

    public TextInline Clone()
    {
        return new TextInline(Text, Font)
        {
            Alignment = Alignment,
            //Decoration = Decoration,
            LineHeight = LineHeight,
            //LetterSpacing = LetterSpacing,
            Fill = Fill,
            FillPaintable = FillPaintable,
            StrokePaintable = StrokePaintable,
            StrokeWidth = StrokeWidth,
        };
    }

    public bool HasEqualSettings(TextInline other, bool ignoreLineSharedSettings = false)
    {
        return (ignoreLineSharedSettings || Alignment == other.Alignment)
               && Math.Abs(LineHeight - other.LineHeight) < float.Epsilon
               && Fill == other.Fill
               && FillPaintable?.GetCacheHash() == other.FillPaintable?.GetCacheHash()
               && StrokePaintable?.GetCacheHash() == other.StrokePaintable?.GetCacheHash()
               && Math.Abs(StrokeWidth - other.StrokeWidth) < float.Epsilon
               && Font.Weight == other.Font.Weight
               && Font.Slant == other.Font.Slant
               && Math.Abs(Font.Size - other.Font.Size) < float.Epsilon
               && Font.Family.Equals(other.Font.Family);
    }
}

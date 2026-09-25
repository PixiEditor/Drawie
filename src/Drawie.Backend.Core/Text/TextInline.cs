using Drawie.Backend.Core.ColorsImpl.Paintables;

namespace Drawie.Backend.Core.Text;

public class TextInline : ICacheable
{
    public string Text { get; set; }
    public FontData Font { get; set; }
    //TODO:
    public RichTextAlign Alignment { get; set; }
    //TODO:
    public TextDecoration Decoration { get; set; }
    public float LineHeight { get; set; }
    public float LetterSpacing { get; set; }
    public bool Fill { get; set; } = true;
    public Paintable? FillPaintable { get; set; }
    public Paintable? StrokePaintable { get; set; }
    public float StrokeWidth { get; set; }
    //TODO:
    // public TextDecorationType DecorationType { get; set; }
    // public float DecorationThicknessPercent { get; set; }
    // public float DecorationOffsetPercent { get; set; }
    // public Paintable DecorationPaintable { get; set; }

    public TextInline(string text, FontData font)
    {
        Text = text;
        Font = font;
    }

    public int GetCacheHash()
    {
        HashCode hash = new HashCode();
        hash.Add(Text);
        hash.Add(Font.GetCacheHash());
        hash.Add(LineHeight);
        hash.Add(LetterSpacing);
        hash.Add(Alignment);
        hash.Add(Decoration);
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
            Alignment = Alignment, Decoration = Decoration, LineHeight = LineHeight, LetterSpacing = LetterSpacing,
            Fill = Fill, FillPaintable = FillPaintable, StrokePaintable = StrokePaintable, StrokeWidth = StrokeWidth,
        };
    }

    public bool HasEqualSettings(TextInline other)
    {
        return Alignment == other.Alignment
               && Decoration == other.Decoration
               && Math.Abs(LineHeight - other.LineHeight) < float.Epsilon
               && Math.Abs(LetterSpacing - other.LetterSpacing) < float.Epsilon
               && Fill == other.Fill
               && FillPaintable?.GetCacheHash() == other.FillPaintable?.GetCacheHash()
               && StrokePaintable?.GetCacheHash() == other.StrokePaintable?.GetCacheHash()
               && Math.Abs(StrokeWidth - other.StrokeWidth) < float.Epsilon
               && Font.Bold == other.Font.Bold
                && Font.Italic == other.Font.Italic
                && Math.Abs(Font.Size - other.Font.Size) < float.Epsilon
                && Font.Family.Equals(other.Font.Family);
    }
}

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
        return hash.ToHashCode();
    }

    public TextInline Clone()
    {
        return new TextInline(Text, Font)
        {
            Alignment = Alignment, Decoration = Decoration, LineHeight = LineHeight, LetterSpacing = LetterSpacing
        };
    }
}

namespace Drawie.Backend.Core.Text;

public record struct FontData : ICacheable
{
    public double Size { get; set; }
    public FontFamilyName Family { get; set; }
    public bool SubPixel { get; set; }
    public FontEdging Edging { get; set; }
    public FontStyleWeight Weight { get; set; }
    public FontStyleSlant Slant { get; set; }
    public FontStyleWidth Width { get; set; }

    public FontData(FontFamilyName family)
    {
        Size = 12;
        Family = family;
        SubPixel = true;
        Edging = FontEdging.AntiAlias;
        Weight = FontStyleWeight.Normal;
        Slant = FontStyleSlant.Upright;
        Width = FontStyleWidth.Normal;
    }

    public static FontData CreateDefault()
    {
        return new FontData(new FontFamilyName("$Default"));
    }

    public Font? ToFont(bool defaultFallback = true)
    {
        Font? font = Font.FromFontFamily(Family, new FontStyle(Weight, Slant, Width));
        if (font == null)
        {
            if (defaultFallback)
            {
                font = Font.FromFontFamily(new FontFamilyName("$Default")) ?? Font.CreateDefault();
            }
            else
            {
                return null;
            }
        }

        font.Size = Size;
        font.SubPixel = SubPixel;
        font.Edging = Edging;
        return font;
    }

    public int GetCacheHash()
    {
        HashCode code = new HashCode();
        code.Add(Size);
        code.Add(SubPixel);
        code.Add(Edging);
        code.Add(Weight);
        code.Add(Slant);
        code.Add(Family.Name);
        code.Add(Family.FontUri != null ? 1 : 0);
        if (Family.FontUri != null)
        {
            code.Add(Family.FontUri.AbsolutePath);
        }

        return code.ToHashCode();
    }
}

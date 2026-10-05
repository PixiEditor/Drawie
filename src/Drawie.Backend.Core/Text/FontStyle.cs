namespace Drawie.Backend.Core.Text;

public record FontStyle
{
    public static FontStyle Normal { get; } = new FontStyle(FontStyleWeight.Normal, FontStyleSlant.Upright, FontStyleWidth.Normal);
    public FontStyleWeight Weight { get; set; }
    public FontStyleSlant Slant { get; set; }
    public FontStyleWidth Width { get; set; }

    public FontStyle(FontStyleWeight weight, FontStyleSlant slant, FontStyleWidth width)
    {
        Weight = weight;
        Slant = slant;
        Width = width;
    }
}

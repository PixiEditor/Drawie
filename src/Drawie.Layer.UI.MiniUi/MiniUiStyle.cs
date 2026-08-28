using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi;

public class MiniUiStyle
{
    public static MiniUiStyle Default { get; } = new MiniUiStyle()
    {
        Foreground = new ColorPaintable(Colors.White),
        BackgroundLow = new ColorPaintable(Color.FromHex("#202020")),
        BackgroundMid = new ColorPaintable(Color.FromHex("#252525")),
        BackgroundHigh =  new ColorPaintable(Color.FromHex("#303030")),
        BorderMid = new ColorPaintable(Color.FromHex("#303030")),
        BorderHigh = new ColorPaintable(Color.FromHex("#404040"))
    };

    public static MiniUiStyle Active { get; set; } = Default;
    
    public Paintable BackgroundLow { get; set; }
    public Paintable BackgroundMid { get; set; }
    public Paintable BackgroundHigh { get; set; }
    public Paintable Foreground { get; set; }
    public Paintable BorderMid { get; set; }
    public Paintable BorderHigh { get; set; }
    public FontFamilyName FontFamily { get; set; } = new FontFamilyName("$Default");
    public float FontSize { get; set; } = 12;
    public float Padding { get; set; } = 4;
    public float Spacing { get; set; } = 8;
    public float Rounding { get; set; } = 2;
    public Font Font => CreateFont();
    public float StrokeThickness { get; set; } = 1.5f;

    private static Font cachedFont;

    private Font CreateFont()
    {
        if (cachedFont == null || cachedFont.Size != FontSize || cachedFont.Family.Name != FontFamily.Name)
        {
            cachedFont?.Dispose();
            var font = Font.FromFontFamily(FontFamily);
            font.Size = FontSize;
            cachedFont =  font;
        }

        return cachedFont;
    }
}
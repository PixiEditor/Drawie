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
    };

    public static MiniUiStyle Active { get; set; } = Default;
    
    public Paintable BackgroundLow { get; set; }
    public Paintable BackgroundMid { get; set; }
    public Paintable Foreground { get; set; }
    public FontFamilyName FontFamily { get; set; } = new FontFamilyName("$Default");
    public float FontSize { get; set; } = 12;
    public float HorizontalPadding { get; set; } = 4;
    public float VerticalPadding { get; set; } = 4;
}
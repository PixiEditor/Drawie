using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class Label
{
    public static void Show(string text)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        RichText rt = new RichText(text)
        {
            Fill = true,
            FillPaintable = MiniUiStyle.Active.Foreground
        };

        Font font = MiniUiStyle.Active.Font;
        using Paint paint = new Paint
        {
            Paintable = MiniUiStyle.Active.Foreground
        };

        VecF size = CalculateBounds(rt, font);

        RectD bounds = new RectD(new VecD(ctx.CurrentPosition.X, ctx.CurrentPosition.Y), (VecD)size);

        if (ctx.Framebuffer != null)
        {
            rt.Paint(ctx.Framebuffer.Canvas, bounds.Pos + new VecD(0, size.Y), font, paint, null);
        }

        Panel.Advance(bounds);
    }

    private static VecF CalculateBounds(RichText label, Font? font)
    {
        return (VecF)label.MeasureBounds(font).Size;
    }
}
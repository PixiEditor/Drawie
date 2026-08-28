using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Host.Input;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class Button
{
    public static bool Show(string label)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null) throw new MiniUiMissingContextException();

        RichText rt = new RichText(label);
        rt.Fill = true;
        Font font = MiniUiStyle.Active.Font;
        
        VecF bounds = CalculateBounds(rt, font);
        
        using Paint btnPaint = new Paint();

        RectD btnBounds = new RectD(ctx.CurrentPosition.X, ctx.CurrentPosition.Y, bounds.X, bounds.Y);
        btnBounds.Size += new VecD(MiniUiStyle.Active.Padding * 2, MiniUiStyle.Active.Padding * 2);

        bool hitTest = btnBounds.ContainsInclusive(MiniUiContext.Active.PointerPosition);
        btnPaint.Paintable = hitTest ? MiniUiStyle.Active.BackgroundHigh : MiniUiStyle.Active.BackgroundMid;
        
        OutlinedRectangle.Draw(btnBounds, btnPaint.Paintable, hitTest ? MiniUiStyle.Active.BorderHigh : MiniUiStyle.Active.BorderMid);
        
        btnPaint.Paintable = MiniUiStyle.Active.Foreground;
        rt.FillPaintable = MiniUiStyle.Active.Foreground;

        bool justPressed = !ctx.LastState.PressedPointerButtons[PointerButton.Left] &&
                           ctx.InputController.PrimaryPointer.IsButtonPressed(PointerButton.Left);

        RectD drawBounds =
            new RectD(
                new VecD(ctx.CurrentPosition.X + MiniUiStyle.Active.Padding,
                    ctx.CurrentPosition.Y + bounds.Y + MiniUiStyle.Active.Padding / 2f), btnBounds.Size);

        if (ctx.Framebuffer != null)
        {
            rt.Paint(ctx.Framebuffer?.Canvas, drawBounds.Pos, font, btnPaint, null);
        }

        Panel.Advance(btnBounds);
        return hitTest && justPressed;
    }

    private static VecF CalculateBounds(RichText label, Font? font)
    {
        return (VecF)label.MeasureBounds(font).Size;
    }
}
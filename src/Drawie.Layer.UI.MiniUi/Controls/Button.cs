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
        using Font font = Font.FromFontFamily(MiniUiStyle.Active.FontFamily);
        
        VecF bounds = CalculateBounds(rt, font);
        
        using Paint btnPaint = new Paint();

        RectD btnBounds = new RectD(ctx.CurrentPosition.X, ctx.CurrentPosition.Y, bounds.X, bounds.Y);
        btnBounds.Size += new VecD(MiniUiStyle.Active.HorizontalPadding * 2, MiniUiStyle.Active.VerticalPadding * 2);

        bool hitTest = btnBounds.ContainsInclusive(MiniUiContext.Active.PointerPosition);
        btnPaint.Paintable = hitTest ? MiniUiStyle.Active.BackgroundLow : MiniUiStyle.Active.BackgroundMid;
        
        ctx.Framebuffer.Canvas!.DrawRect(btnBounds, btnPaint);
        
        btnPaint.Paintable = MiniUiStyle.Active.Foreground;
        rt.FillPaintable = MiniUiStyle.Active.Foreground;

        bool justPressed = !ctx.LastState.PressedPointerButtons[PointerButton.Left] &&
                           ctx.InputController.PrimaryPointer.IsButtonPressed(PointerButton.Left);
        
        rt.Paint(ctx.Framebuffer.Canvas, new VecD(ctx.CurrentPosition.X + MiniUiStyle.Active.HorizontalPadding, ctx.CurrentPosition.Y + bounds.Y + MiniUiStyle.Active.VerticalPadding / 2f), font, btnPaint,null);
        return hitTest && justPressed;
    }

    private static VecF CalculateBounds(RichText label, Font? font)
    {
        return (VecF)label.MeasureBounds(font).Size;
    }
}
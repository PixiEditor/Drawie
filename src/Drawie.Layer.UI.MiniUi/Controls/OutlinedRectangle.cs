using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class OutlinedRectangle
{
    public static void Draw(RectD bounds, Paintable fill, Paintable stroke)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();
        
        using var paint = new Paint();
        
        paint.Style = PaintStyle.StrokeAndFill;
        paint.Paintable = stroke;

        double strokeThickness = MiniUiStyle.Active.StrokeThickness;
        double radius = MiniUiStyle.Active.Rounding;

        var strokeBounds = bounds.Inflate(strokeThickness);

        ctx.Framebuffer?.Canvas!.DrawRoundRect(
            (float)strokeBounds.X,
            (float)strokeBounds.Y,
            (float)strokeBounds.Width,
            (float)strokeBounds.Height,
            (float)(radius + strokeThickness),
            (float)(radius + strokeThickness),
            paint);

        paint.Style = PaintStyle.Fill;
        paint.Paintable = fill;

        ctx.Framebuffer?.Canvas!.DrawRoundRect(
            (float)bounds.X,
            (float)bounds.Y,
            (float)bounds.Width,
            (float)bounds.Height,
            (float)radius,
            (float)radius,
            paint);
        
    }
}
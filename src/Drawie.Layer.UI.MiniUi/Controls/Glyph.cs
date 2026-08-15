using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class Glyph
{
    public static void Draw(string svg, double size = 14)
    {
        MiniUiContext? ctx = MiniUiContext.Active;

        if (ctx == null)
            throw new MiniUiMissingContextException();

        using var path = VectorPath.FromSvgPath(svg);

        RectD bounds = new RectD(
            ctx.CurrentPosition.X,
            ctx.CurrentPosition.Y,
            size,
            size);

        RectD pathBounds = path.Bounds;

        if (pathBounds.Width > 0 && pathBounds.Height > 0)
        {
            double scale = Math.Min(
                size / pathBounds.Width,
                size / pathBounds.Height);

            double scaledWidth = pathBounds.Width * scale;
            double scaledHeight = pathBounds.Height * scale;

            double x = bounds.X + (size - scaledWidth) / 2;
            double y = bounds.Y + (size - scaledHeight) / 2;

            var transform =
                Matrix3X3.CreateScale((float)scale, (float)scale);

            path.Transform(transform);

            RectD scaledBounds = path.Bounds;

            path.Offset(
                new VecD(
                    x - scaledBounds.X,
                    y - scaledBounds.Y));
        }

        using Paint paint = new Paint
        {
            Paintable = MiniUiStyle.Active.Foreground
        };

        ctx.Framebuffer?.Canvas!.DrawPath(path, paint);

        Panel.Advance(bounds);
    }
}
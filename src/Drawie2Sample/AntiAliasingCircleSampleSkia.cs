using Drawie.Backend.Arco;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using Drawie.Rendering;
using Paint = Drawie.Backend.Core.Surfaces.PaintImpl.Paint;

namespace Drawie2Sample;

public static class AntiAliasingCircleSampleSkia
{
    private static Texture texture;

    public static void Draw(TextureFramebuffer fb)
    {
        if (texture == null)
        {
            texture = Texture.ForDisplay(new VecI(fb.Size.X / 4, fb.Size.Y / 4));
            texture.DrawingSurface.Canvas.DrawCircle(100, 100, 50, new Paint()
            {
                Color = Colors.Green,
                IsAntiAliased = true
            });
        }

        fb.Canvas.Save();
        fb.Canvas.Scale(4);
        fb.Canvas.DrawSurface(texture.DrawingSurface, 0, 0);
        fb.Canvas.Restore();
    }
}
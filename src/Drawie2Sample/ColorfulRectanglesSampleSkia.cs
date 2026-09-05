using Drawie.Backend.Arco;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Rendering;

namespace Drawie2Sample;

public static class ColorfulRectanglesSampleSkia
{
    private static Texture texture;

    public static void Draw(TextureFramebuffer fb)
    {
        if (texture == null)
        {
            texture = Texture.ForDisplay(fb.Size);

            const int columns = 50;
            const int rows = 30;
            const float size = 20;
            const float spacing = 4;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    texture.DrawingSurface.Canvas.DrawRect(
                        x * (size + spacing),
                        y * (size + spacing),
                        size,
                        size,
                        new Drawie.Backend.Core.Surfaces.PaintImpl.Paint() { Color = new Color((byte)(x * 255 / columns), (byte)(y * 255 / rows), 100, 255) });
                }
            }
        }
        
        fb.Canvas.DrawSurface(texture.DrawingSurface, 0, 0);
    }
}
using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.Rendering;
using Canvas = Drawie.Backend.Arco.Canvas;

namespace Drawie2Sample;

public static class ColorfulCirclesSample
{
    static Drawie.Backend.Arco.Canvas cnvs = null;

    public static void Draw(TextureFramebuffer fb)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice, fb.Size);
        }

        const int columns = 50;
            const int rows = 30;
            const float size = 20;
            const float spacing = 4;

            int i = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    RectD rect = new RectD(x * (size + spacing), y * (size + spacing), size, size);
                    cnvs.DrawCircle((float)rect.Center.X, (float)rect.Center.Y, size / 2f,
                        new Paint
                        {
                            Color = new Color((byte)(x * 255 / columns), (byte)(y * 255 / rows), 100, 255),
                            IsAntiAliased = i % 2 == 0,
                            BlendMode = BlendMode.Src
                        });
                    i++;
                }
            }

            cnvs.Flush(fb);
    }
}
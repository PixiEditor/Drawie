using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Rendering;

namespace Drawie2Sample;

public static class ColorfulRectanglesSample
{
    static Drawie.Backend.Arco.Canvas cnvs = null;

    public static void Draw(TextureFramebuffer fb)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice, fb, fb.Size);
        }
        else
        {
            cnvs.renderTarget = fb;
        }

        const int columns = 50;
        const int rows = 30;
        const float size = 20;
        const float spacing = 4;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                cnvs.DrawRect(
                    x * (size + spacing),
                    y * (size + spacing),
                    size,
                    size,
                    new Paint { Color = new Color((byte)(x * 255 / columns), (byte)(y * 255 / rows), 100, 255) });
            }
        }

        cnvs.Flush();
    }
}
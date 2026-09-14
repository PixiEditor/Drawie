using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie2Sample;

public class ColorfulRectanglesSample : ArcoSample
{
    public ColorfulRectanglesSample(ArcoGraphicsContext context, VecI size) : base(context, size)
    {
    }

    public override void OnInit()
    {
        const int columns = 50;
        const int rows = 30;
        const float size = 20;
        const float spacing = 4;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                RenderSurface.Canvas.DrawRect(
                    x * (size + spacing),
                    y * (size + spacing),
                    size,
                    size,
                    new Paint { Color = new Color((byte)(x * 255 / columns), (byte)(y * 255 / rows), 100, 255) });
            }
        }
    }
}
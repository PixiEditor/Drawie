using Drawie.Backend.Arco;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Rendering;
using Paint = Drawie.Backend.Core.Surfaces.PaintImpl.Paint;

public static class BlendingSampleSkia
{
    static Texture cnvs = null;

    private static readonly BlendMode[] HardwareBlendModes =
    [
        BlendMode.Src,
        BlendMode.SrcOver,
        BlendMode.DstIn,
        BlendMode.Dst,
        BlendMode.SrcOut,
        BlendMode.DstOut,
        BlendMode.DstOver,
        BlendMode.SrcIn,
        BlendMode.SrcATop,
        BlendMode.DstATop,
        BlendMode.Xor,
        BlendMode.Plus
    ];


    public static void Draw(TextureFramebuffer target)
    {
        if (cnvs == null)
        {
            cnvs = new Texture(target.Size);
            const int columns = 4;
            const float padding = 20;
            const float labelHeight = 30;

            float cellWidth = target.Size.X / (float)columns;
            int rows = (HardwareBlendModes.Length + columns - 1) / columns;
            float cellHeight = target.Size.Y / rows;

            for (int i = 0; i < HardwareBlendModes.Length; i++)
            {
                var mode = HardwareBlendModes[i];

                int column = i % columns;
                int row = i / columns;

                float cellX = column * cellWidth;
                float cellY = row * cellHeight;

                float size = Math.Min(cellWidth, cellHeight - labelHeight) * 0.55f;

                float centerX = cellX + cellWidth / 2f;
                float centerY = cellY + labelHeight + (cellHeight - labelHeight) / 2f;

                cnvs.DrawingSurface.Canvas.DrawRect(
                    centerX - size * 0.65f,
                    centerY - size * 0.5f,
                    size,
                    size,
                    new Drawie.Backend.Core.Surfaces.PaintImpl.Paint()
                    {
                        Color = Colors.Red.WithAlpha(128),
                    });

                cnvs.DrawingSurface.Canvas.DrawRect(
                    centerX - size * 0.15f,
                    centerY - size * 0.5f,
                    size,
                    size,
                    new Paint()
                    {
                        Color = Colors.Green.WithAlpha(128),
                        BlendMode = mode
                    });
            }

            cnvs.DrawingSurface.Flush();
        }
        
        target.Canvas.DrawSurface(cnvs.DrawingSurface, 0, 0);
    }
}
using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.Rendering;
using Canvas = Drawie.Backend.Arco.Canvas;

public static class BlendingSample
{
    static Canvas cnvs = null;
    
    private static readonly BlendMode[] HardwareBlendModes =
    [
        BlendMode.Src,
        BlendMode.Dst,
        BlendMode.SrcOver,
        BlendMode.DstOver,
        BlendMode.SrcIn,
        BlendMode.DstIn,
        BlendMode.SrcOut,
        BlendMode.DstOut,
        BlendMode.SrcATop,
        BlendMode.DstATop,
        BlendMode.Xor,
        BlendMode.Plus
    ];

    
    public static void Draw(TextureFramebuffer fb)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(
                DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice,
                fb,
                fb.Size);
        }
        else
        {
            cnvs.renderTarget = fb;
        }
        
        const int columns = 4;
        const float padding = 20;
        const float labelHeight = 30;

        float cellWidth = fb.Size.X / (float)columns;
        int rows = (HardwareBlendModes.Length + columns - 1) / columns;
        float cellHeight = fb.Size.Y / rows;

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

            fb.Canvas.DrawText(
                mode.ToString(),
                new VecD(cellX + padding,
                cellY + padding),
                new Drawie.Backend.Core.Surfaces.PaintImpl.Paint() { Color = Colors.White });

            cnvs.DrawRect(
                centerX - size * 0.65f,
                centerY - size * 0.5f,
                size,
                size,
                new Paint
                {
                    Color = Colors.Red.WithAlpha(128),
                    BlendMode = BlendMode.SrcOver
                });

            cnvs.DrawRect(
                centerX - size * 0.15f,
                centerY - size * 0.5f,
                size,
                size,
                new Paint
                {
                    Color = Colors.Green.WithAlpha(128),
                    BlendMode = mode
                });
        }

        cnvs.Flush();
    }
}
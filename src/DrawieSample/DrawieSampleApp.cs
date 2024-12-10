using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.Windowing;
using DrawiEngine;

namespace DrawieSample;

public class DrawieSampleApp : DrawieApp
{
    private IWindow window;

    public override IWindow CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));
        return window;
    }

    protected override void OnInitialize()
    {
        Paint paint = new Paint() { IsAntiAliased = true };

        Texture testTexture = new Texture(new VecI(800, 600));
        DrawHorizontalColorStrips(testTexture, paint);

        DrawBlendTestHorizontalStrips(testTexture, paint);
        
        DrawingSurface srgbSurface = DrawingSurface.Create(new ImageInfo(testTexture.Size.X, testTexture.Size.Y,
            ColorType.Rgba8888, AlphaType.Premul, ColorSpace.CreateSrgb()) { GpuBacked = true });
        
        srgbSurface.Canvas.DrawSurface(testTexture.DrawingSurface, 0, 0);

        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.DrawingSurface.Canvas.Clear(Colors.White);
            targetTexture.DrawingSurface.Canvas.DrawSurface(srgbSurface, 0, 0);
            DrawReferenceColors(targetTexture, paint);
        };
    }

    private void DrawReferenceColors(Texture targetTexture, Paint paint)
    {
        using Paint referencePaint = new Paint() { IsAntiAliased = true };
        referencePaint.Color = Colors.Black;
        targetTexture.DrawingSurface.Canvas.DrawRect(0, 0, 5, 5, referencePaint);
        referencePaint.Color = Colors.White;
        targetTexture.DrawingSurface.Canvas.DrawRect(5, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(255, 0, 0);
        targetTexture.DrawingSurface.Canvas.DrawRect(10, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(0, 255, 0);
        targetTexture.DrawingSurface.Canvas.DrawRect(15, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(0, 0, 255);
        targetTexture.DrawingSurface.Canvas.DrawRect(20, 0, 5, 5, referencePaint);
    }

    private void DrawHorizontalColorStrips(Texture targetTexture, Paint paint)
    {
        int stripWidth = targetTexture.Size.X / 4;
        int stripHeight = targetTexture.Size.Y;

        int spacing = 10;

        Color[] colors = [Color.FromRgb(0, 255, 0), Colors.Yellow, Colors.Cyan, Colors.Magenta];

        for (int i = 0; i < 4; i++)
        {
            paint.Color = colors[i];
            targetTexture.DrawingSurface.Canvas.DrawRect(i * stripWidth + spacing, spacing, stripWidth - 2 * spacing,
                stripHeight, paint);
        }
    }

    private void DrawBlendTestHorizontalStrips(Texture targetTexture, Paint paint)
    {
        int stripWidth = targetTexture.Size.X;
        int stripHeight = targetTexture.Size.Y / 3;

        int spacing = 50;

        Color[] colors = [Colors.Red, Colors.Blue, Colors.Green];

        for (int i = 0; i < 3; i++)
        {
            paint.Color = colors[i].WithAlpha(128);
            paint.Style = PaintStyle.Fill;
            targetTexture.DrawingSurface.Canvas.DrawRect(0, i * stripHeight + spacing, stripWidth,
                stripHeight - 2 * spacing, paint);
        }
    }
}

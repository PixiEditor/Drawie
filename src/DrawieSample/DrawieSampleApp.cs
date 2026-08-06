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

        NativeTexture testNativeTexture = new NativeTexture(new VecI(800, 600));
        DrawHorizontalColorStrips(testNativeTexture, paint);

        DrawBlendTestHorizontalStrips(testNativeTexture, paint);
        
        DrawingSurface srgbSurface = DrawingSurface.Create(new ImageInfo(testNativeTexture.Size.X, testNativeTexture.Size.Y,
            ColorType.Rgba8888, AlphaType.Premul, ColorSpace.CreateSrgb()) { GpuBacked = true });
        
        srgbSurface.Canvas.DrawSurface(testNativeTexture.DrawingSurface, 0, 0);

        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.DrawingSurface.Canvas.Clear(Colors.White);
            targetTexture.DrawingSurface.Canvas.DrawSurface(srgbSurface, 0, 0);
            DrawReferenceColors(targetTexture, paint);
        };
    }

    private void DrawReferenceColors(NativeTexture targetNativeTexture, Paint paint)
    {
        using Paint referencePaint = new Paint() { IsAntiAliased = true };
        referencePaint.Color = Colors.Black;
        targetNativeTexture.DrawingSurface.Canvas.DrawRect(0, 0, 5, 5, referencePaint);
        referencePaint.Color = Colors.White;
        targetNativeTexture.DrawingSurface.Canvas.DrawRect(5, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(255, 0, 0);
        targetNativeTexture.DrawingSurface.Canvas.DrawRect(10, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(0, 255, 0);
        targetNativeTexture.DrawingSurface.Canvas.DrawRect(15, 0, 5, 5, referencePaint);
        referencePaint.Color = Color.FromRgb(0, 0, 255);
        targetNativeTexture.DrawingSurface.Canvas.DrawRect(20, 0, 5, 5, referencePaint);
    }

    private void DrawHorizontalColorStrips(NativeTexture targetNativeTexture, Paint paint)
    {
        int stripWidth = targetNativeTexture.Size.X / 4;
        int stripHeight = targetNativeTexture.Size.Y;

        int spacing = 10;

        Color[] colors = [Color.FromRgb(0, 255, 0), Colors.Yellow, Colors.Cyan, Colors.Magenta];

        for (int i = 0; i < 4; i++)
        {
            paint.Color = colors[i];
            targetNativeTexture.DrawingSurface.Canvas.DrawRect(i * stripWidth + spacing, spacing, stripWidth - 2 * spacing,
                stripHeight, paint);
        }
    }

    private void DrawBlendTestHorizontalStrips(NativeTexture targetNativeTexture, Paint paint)
    {
        int stripWidth = targetNativeTexture.Size.X;
        int stripHeight = targetNativeTexture.Size.Y / 3;

        int spacing = 50;

        Color[] colors = [Colors.Red, Colors.Blue, Colors.Green];

        for (int i = 0; i < 3; i++)
        {
            paint.Color = colors[i].WithAlpha(128);
            paint.Style = PaintStyle.Fill;
            targetNativeTexture.DrawingSurface.Canvas.DrawRect(0, i * stripHeight + spacing, stripWidth,
                stripHeight - 2 * spacing, paint);
        }
    }
}

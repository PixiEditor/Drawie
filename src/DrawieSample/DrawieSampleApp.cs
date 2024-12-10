using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
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
        Paint paint = new Paint() { Color = Color.FromRgb(0, 255, 0), Style = PaintStyle.StrokeAndFill, IsAntiAliased = true };

        Texture testTexture = new Texture(new VecI(800, 600));
        DrawHorizontalColorStrips(testTexture, paint);
        DrawBlendTestHorizontalStrips(testTexture, paint);

        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.DrawingSurface.Canvas.Clear(Colors.White);

            targetTexture.DrawingSurface.Canvas.DrawSurface(testTexture.DrawingSurface, 0, 0);
        };
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

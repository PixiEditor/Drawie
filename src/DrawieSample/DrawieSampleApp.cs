using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Windowing;
using DrawiEngine;
using PixiEditor.Numerics;

namespace DrawieSample;

public class DrawieSampleApp : DrawieApp
{
    private IWindow window;
    public override IWindow CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));
        return window;
    }

    public override void Run()
    {
        Paint paint = new Paint()
        {
            Color = Colors.Green,
            Style = PaintStyle.StrokeAndFill
        };
        
        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.DrawingSurface.Canvas.Clear(Colors.LightCoral);
            targetTexture.DrawingSurface.Canvas.DrawRect(0, 0, 100, 100, paint);
        };
    }
}
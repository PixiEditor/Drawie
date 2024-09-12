using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using DrawiEngine;
using PixiEditor.Numerics;

DrawingEngine engine = DrawingEngine.CreateDefaultDesktop();

var window = engine.WindowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));

Paint paint = new Paint()
{
   Color = Colors.Green,
   Style = PaintStyle.StrokeAndFill
};

window.Render += (targetTexture, deltaTime) =>
{
   targetTexture.DrawingSurface.Canvas.Clear(Colors.LightCoral);
   targetTexture.DrawingSurface.Canvas.DrawCircle(100, 100, 100, paint);
};

engine.RunWithWindow(window);

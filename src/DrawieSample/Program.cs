using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces.PaintImpl;
using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;
using DrawiEngine;
using PixiEditor.Numerics;
DrawingEngine engine = DrawingEngine.CreateDefault();

var window = engine.WindowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));

Paint paint = new Paint()
{
   Color = Colors.Green,
   Style = PaintStyle.StrokeAndFill
};

window.Update += (deltaTime) =>
{
   if (engine.WindowingPlatform.Windows.Count == 1)
   {
      var window2 = engine.WindowingPlatform.CreateWindow("Drawie Sample 2", new VecI(800, 600));
      window2.Show();
   }
};

window.Render += (targetTexture, deltaTime) =>
{
   targetTexture.DrawingSurface.Canvas.Clear(Colors.LightCoral);
   targetTexture.DrawingSurface.Canvas.DrawCircle(100, 100, 100, paint);
};

engine.RunWithWindow(window);

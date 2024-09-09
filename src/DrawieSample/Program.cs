using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces.PaintImpl;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.WebGpu;
using Drawie.Silk;
using Drawie.Skia;
using DrawiEngine;
using PixiEditor.Numerics;

//DrawingEngine engine = DrawingEngine.CreateDefault();

var renderApi = new WebGpuRenderApi();
DrawingEngine engine = new DrawingEngine(renderApi, new GlfwWindowingPlatform(renderApi), new SkiaDrawingBackend());


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

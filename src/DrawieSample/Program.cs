using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces.PaintImpl;
using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;
using DrawiEngine;
using PixiEditor.Numerics;
using SkiaSharp;

SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
VulkanRenderApi renderApi = new VulkanRenderApi();
GlfwWindowingPlatform windowingPlatform = new GlfwWindowingPlatform(renderApi);

DrawingEngine engine = new DrawingEngine(windowingPlatform, drawingBackend);
engine.Run();

var window = windowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));


window.Update += deltaTime =>
{
    
};

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

window.Show();
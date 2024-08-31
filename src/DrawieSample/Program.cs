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

var window = windowingPlatform.CreateWindow("Drawie Sample", new VecI(800, 600));


/*window.Update += deltaTime =>
{
    
};

window.Render += deltaTime =>
{
    
};*/

window.Show();
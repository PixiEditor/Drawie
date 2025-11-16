using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using DrawiEngine;

VulkanOffscreenContext context = new VulkanOffscreenContext();
context.Initialize(new OffscreenVulkanContextInfo());
VulkanRenderApi renderApi = new VulkanRenderApi(context);
DrawingEngine engine = new DrawingEngine(renderApi, null, new SkiaDrawingBackend(), new DrawieRenderingDispatcher());

engine.Run();

engine.RenderingDispatcher.Enqueue(() =>
{
    Texture texture = new Texture(new VecI(512, 512));
    texture.DrawingSurface.Canvas.Clear(Colors.CornflowerBlue);
    texture.DrawingSurface.Canvas.DrawCircle(256, 256, 200,
        new Paint() { Color = Colors.OrangeRed, IsAntiAliased = true });

    Surface surf = new Surface(texture.Size);
    surf.DrawingSurface.Canvas.DrawSurface(texture.DrawingSurface, 0, 0);
    surf.SaveToDesktop();
});


await engine.WaitForIdleAsync();

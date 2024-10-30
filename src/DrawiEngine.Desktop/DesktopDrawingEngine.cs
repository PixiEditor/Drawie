using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;

namespace DrawiEngine.Desktop;

public static class DesktopDrawingEngine
{
    public static DrawingEngine CreateDefaultDesktop()
    {
        VulkanRenderApi renderApi = new VulkanRenderApi();
        return new DrawingEngine(renderApi, new GlfwWindowingPlatform(renderApi), new SkiaDrawingBackend(), new DrawieRenderingDispatcher());
    }
}

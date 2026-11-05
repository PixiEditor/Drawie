using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;

namespace DrawiEngine.Desktop;

public static class DesktopDrawingEngine
{
    public static DrawingEngine CreateDefaultDesktop(bool preferVulkan = true)
    {
        IRenderApi renderApi = preferVulkan ? new VulkanRenderApi() : new OpenGlRenderApi();

        if (OperatingSystem.IsMacOS())
        {
            renderApi = new OpenGlRenderApi();
        }

        return new DrawingEngine(renderApi, new GlfwWindowingPlatform(renderApi), new SkiaDrawingBackend(),
            new DrawieRenderingDispatcher());
    }
}

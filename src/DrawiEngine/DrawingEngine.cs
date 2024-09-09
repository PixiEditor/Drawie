using Drawie.Core.Bridge;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;
using Drawie.Windowing;

namespace DrawiEngine;

public class DrawingEngine
{
     public IRenderApi RenderApi { get; }
     public IWindowingPlatform WindowingPlatform { get; } 
     public IDrawingBackend DrawingBackend { get; }
     
     public DrawingEngine(IRenderApi renderApi, IWindowingPlatform windowingPlatform,
          IDrawingBackend drawingBackend)
     {
          RenderApi = renderApi;
          WindowingPlatform = windowingPlatform;
          DrawingBackend = drawingBackend;

          DrawingBackendApi.SetupBackend(DrawingBackend, new DrawieRenderingDispatcher());
     }
     
     public static DrawingEngine CreateDefault()
     {
          VulkanRenderApi renderApi = new VulkanRenderApi();
          return new DrawingEngine(renderApi, new GlfwWindowingPlatform(renderApi), new SkiaDrawingBackend());
     }

     public void RunWithWindow(IWindow window)
     {
          Console.WriteLine("Running DrawieEngine with configuration:");
          Console.WriteLine($"\t- RenderApi: {RenderApi.GraphicsApi}");
          Console.WriteLine($"\t- WindowingPlatform: {WindowingPlatform}");
          Console.WriteLine($"\t- DrawingBackend: {DrawingBackend}");
          
          window.Initialize();
          
          DrawingBackendApi.InitializeBackend(RenderApi);
          window.Show();
     }
}

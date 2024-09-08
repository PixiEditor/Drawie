using Drawie.Core.Bridge;
using Drawie.RenderApi;
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

     public void RunWithWindow(IWindow window)
     {
          DrawingBackendApi.InitializeBackend(RenderApi);
          window.Show();
     }
}

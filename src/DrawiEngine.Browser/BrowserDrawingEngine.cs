using Drawie.Html5Canvas;
using Drawie.RenderApi.Html5Canvas;
using Drawie.Windowing.Browser;

namespace DrawiEngine.Browser;

public static class BrowserDrawingEngine
{
     public static DrawingEngine CreateDefaultBrowser()
     {
          Html5CanvasRenderApi renderApi = new Html5CanvasRenderApi();
          return new DrawingEngine(renderApi, new BrowserWindowingPlatform(renderApi), new HtmlCanvasDrawingBackend());
     }
}
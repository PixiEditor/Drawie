using Drawie.JSInterop;
using Drawie.RenderApi.WebGl;
using Drawie.Skia;
using Drawie.Windowing.Browser;

namespace DrawiEngine.Browser;

public static class BrowserDrawingEngine
{
     public static DrawingEngine CreateDefaultBrowser()
     {
         WebGlRenderApi renderApi = new WebGlRenderApi();
         return new DrawingEngine(renderApi, new BrowserWindowingPlatform(renderApi), new SkiaDrawingBackend(), new DrawieRenderingDispatcher());
     }
}

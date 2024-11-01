using Drawie.JSInterop;
using Drawie.Numerics;

namespace Drawie.RenderApi.Html5Canvas;

public class Html5CanvasWindowApi : IBrowserWindowRenderApi
{
    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;
    public ITexture RenderTexture => canvasObject;

    public string CanvasId { get; private set; }

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());
        
        JSRuntime.OpenCanvasContext(CanvasId, "webgl");
    }

    public void DestroyInstance()
    {
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        canvasObject.SetAttribute("width", width.ToString());
        canvasObject.SetAttribute("height", height.ToString());
        FramebufferResized?.Invoke();
    }

    public void PrepareTextureToWrite()
    {
    }

    public void Render(double deltaTime)
    {
    }
}

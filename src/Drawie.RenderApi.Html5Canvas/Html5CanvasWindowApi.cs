using Drawie.JSInterop;
using PixiEditor.Numerics;

namespace Drawie.RenderApi.Html5Canvas;

public class Html5CanvasWindowApi : IBrowserWindowRenderApi
{
    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;

    public string CanvasId { get; private set; }
    
    public void CreateInstance(object surface, VecI framebufferSize)
    {
        canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());
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
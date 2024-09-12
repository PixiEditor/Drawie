using Drawie.JSInterop;
using PixiEditor.Numerics;

namespace Drawie.RenderApi.Html5Canvas;

public class Html5CanvasWindowApi : IWindowRenderApi
{
    public event Action? FramebufferResized;
    
    public void CreateInstance(object surface, VecI framebufferSize)
    {
        HtmlObject canvasObject = JSRuntime.CreateElement("canvas");
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());
    }

    public void DestroyInstance()
    {
        
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        
    }

    public void PrepareTextureToWrite()
    {
        
    }

    public void Render(double deltaTime)
    {
        
    }
}
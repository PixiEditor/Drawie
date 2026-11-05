using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Web.Common;
using Drawie.RenderApi.WebGl.Enums;
using Drawie.RenderApi.WebGl.Exceptions;

namespace Drawie.RenderApi.WebGl;

public class WebGlHostViewRenderApi : IHostViewRenderApi
{
    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;
    public ITexture RenderTexture => texture;

    public string CanvasId { get; private set; }

    public event Action InstanceCreated;
    
    public int gl;

    private WebGlRenderTarget texture;
    private int framebuffer;
    
    private VecI fbSize;

    public IGraphicsContext GraphicsContext => webglGraphicsContext;
    private WebGlGraphicsContext webglGraphicsContext;

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        if(contextObject is not HtmlCanvas canvas) throw new ArgumentException("Canvas not found", nameof(contextObject));
        
        canvasObject = canvas;
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());

        gl = JSRuntime.OpenSkiaContext(canvasObject.Id);
        webglGraphicsContext = new WebGlGraphicsContext(gl);
        
        JSRuntime.MakeContextCurrent(gl);

        texture = webglGraphicsContext.CreateRenderTarget(gl, framebufferSize.X, framebufferSize.Y);
        fbSize = framebufferSize;
        InstanceCreated?.Invoke();
    }

    public void DestroyInstance()
    {
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        canvasObject.SetAttribute("width", width.ToString());
        canvasObject.SetAttribute("height", height.ToString());
        fbSize = new VecI(width, height);
        FramebufferResized?.Invoke();
    }

    public void PrepareTextureToWrite()
    {
    }

    public void Render(double deltaTime)
    {
    }
}

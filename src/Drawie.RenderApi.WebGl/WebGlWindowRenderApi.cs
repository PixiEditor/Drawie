using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi.Html5Canvas;
using Drawie.RenderApi.WebGl.Enums;
using Drawie.RenderApi.WebGl.Exceptions;

namespace Drawie.RenderApi.WebGl;

public class WebGlWindowRenderApi : IWindowRenderApi
{
    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;
    public ITexture RenderTexture => texture;

    public string CanvasId { get; private set; }
    
    public int gl;

    private WebGlTexture texture;
    private int framebuffer;
    
    private VecI fbSize;

    public IGraphicsContext GraphicsContext => webglGraphicsContext;
    private WebGlGraphicsContext webglGraphicsContext;

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        JSRuntime.InterceptGLObject();
        canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());

        gl = JSRuntime.OpenSkiaContext(canvasObject.Id);
        webglGraphicsContext = new WebGlGraphicsContext(gl);
        
        JSRuntime.MakeContextCurrent(gl);

        texture = CreateFramebuffer(gl, framebufferSize.X, framebufferSize.Y);
        fbSize = framebufferSize;
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

    private WebGlTexture CreateFramebuffer(int handle, int width, int height)
    {
        var tex = webglGraphicsContext.CreateTexture(handle, width, height);
        framebuffer = JSRuntime.CreateFramebuffer(gl);

        JSRuntime.BindFramebuffer(
            gl,
            framebuffer);
        
        JSRuntime.FramebufferTexture2D(
            gl,
            (int)WebGlFramebufferTarget.Framebuffer,
            (int)WebGlFramebufferAttachment.ColorAttachment0,
            (int)WebGlTextureType.Texture2D,
            tex.TextureId,
            0);
        
        var status = JSRuntime.CheckFramebufferStatus(gl, (int)WebGlFramebufferTarget.Framebuffer);
        
        if (status != (int)WebGlFramebufferStatus.FramebufferComplete)
        {
            WebGlError error = (WebGlError)JSRuntime.GetError(gl);
            throw new Exception($"Framebuffer invalid: {status}, Error: {error}");
        }

        return tex;
    }
    
    private void DisposeTexture()
    {
        JSRuntime.BindFramebuffer(gl, 0);
        
        JSRuntime.DeleteFramebuffer(gl, framebuffer);
        framebuffer = 0;
        webglGraphicsContext.DisposeTexture(texture);
    }
}

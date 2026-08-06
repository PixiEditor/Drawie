using Drawie.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlWindowRenderApi : IOpenGlWindowRenderApi
{
    public event Action? FramebufferResized;
    public ITexture RenderTexture => texture;

    public IGLContext Context { get; private set; }

    private GL Api { get; set; }
    
    private OpenGlTexture texture;

    public IGraphicsContext GraphicsContext { get; private set; }

    public unsafe void CreateInstance(object contextObject, VecI framebufferSize)
    {
        if (contextObject is not IGLContext glContext)
            throw new ArgumentException("contextObject must be an INativeWindow");

        Context = glContext;
        Api = GL.GetApi(glContext);
        var graphicsContext = new OpenGlGraphicsContext(Api, glContext);
        texture = graphicsContext.CreateTexture(0); // default framebuffer texture
        GraphicsContext = graphicsContext;
    }

    public void DestroyInstance()
    {
        Api = null;
        GraphicsContext = null;
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        FramebufferResized?.Invoke();
    }

    public void PrepareTextureToWrite()
    {
    }

    public void Render(double deltaTime)
    {
    }
}

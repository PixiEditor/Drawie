using System.Drawing;
using Drawie.Numerics;
using Drawie.RenderApi.OpenGL.Exceptions;
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

    public unsafe void CreateInstance(object contextObject, VecI framebufferSize)
    {
        if (contextObject is not IGLContext glContext)
            throw new ArgumentException("contextObject must be an INativeWindow");

        Context = glContext;
        Api = GL.GetApi(glContext);
        texture = new OpenGlTexture(0, Api); // default framebuffer texture
    }

    public void DestroyInstance()
    {
        Api = null;
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

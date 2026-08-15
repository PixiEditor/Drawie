using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public sealed class OpenGlRenderTarget : IDisposable, IRenderTarget
{
    public OpenGlTexture Color { get; }
    public OpenGlDepthBuffer? Depth { get; }

    public ulong SurfaceId { get; }
    public VecI Size => new VecI(Width, Height);

    public int Width => Color.Width;
    public int Height => Color.Height;

    private GL Api { get; }

    public OpenGlRenderTarget(
        GL api,
        int width,
        int height,
        DepthFormat depth)
    {
        Api = api;

        Color = new OpenGlTexture(
            api,
            width,
            height);

        if (depth != DepthFormat.NoDepth)
            Depth = new OpenGlDepthBuffer(
                api,
                width,
                height, depth);

        SurfaceId = Api.GenFramebuffer();

        Api.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            (uint)SurfaceId);

        Api.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            (uint)Color.TextureId,
            0);

        if (Depth != null)
        {
            Api.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer,
                Depth.RenderbufferId);
        }

        var status = Api.CheckFramebufferStatus(
            FramebufferTarget.Framebuffer);

        if (status != GLEnum.FramebufferComplete)
        {
            throw new InvalidOperationException(
                $"OpenGL framebuffer is incomplete: {status}");
        }

        Api.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            0);
    }

    public void Bind()
    {
        Api.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            (uint)SurfaceId);
    }

    public void Dispose()
    {
        Api.DeleteFramebuffer((uint)SurfaceId);

        Depth?.Dispose();
        Color.Dispose();
    }
}
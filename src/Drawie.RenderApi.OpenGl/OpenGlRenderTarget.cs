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
        TextureDesc desc)
    {
        Api = api;

        Color = new OpenGlTexture(
            api,
            desc.Width,
            desc.Height, desc.Samples);

        if (desc.Depth != DepthFormat.NoDepth)
            Depth = new OpenGlDepthBuffer(
                api,
                desc.Width,
                desc.Height, desc.Depth, desc.Samples);

        SurfaceId = Api.GenFramebuffer();

        Api.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            (uint)SurfaceId);

        var target = desc.Samples == 1 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample;
        Api.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            target,
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

    public void Dispose()
    {
        Api.DeleteFramebuffer((uint)SurfaceId);

        Depth?.Dispose();
        Color.Dispose();
    }
}
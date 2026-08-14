using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlRenderTarget : IRenderTarget, IWebGlTexture, IDisposable
{
    public int Gl { get; }
    public uint FramebufferId { get; }
    public VecI Size { get; }
    public ulong TextureId => texture.TextureId;

    private WebGlTexture texture;
    private WebGlDepthBuffer depthBuffer;

    public WebGlRenderTarget(int gl, int width, int height, DepthFormat depth)
    {
        Gl = gl;
        Size = new VecI(width, height);
        texture = new WebGlTexture(gl, width, height);
        FramebufferId = (uint)JSRuntime.CreateFramebuffer(gl);

        if (depth != DepthFormat.NoDepth)
        {
            depthBuffer = new WebGlDepthBuffer(gl, width, height, depth);
        }

        JSRuntime.BindFramebuffer(gl, (int)WebGlFramebufferTarget.Framebuffer, (int)FramebufferId);
        
        JSRuntime.FramebufferTexture2D(gl,
            (int)WebGlFramebufferTarget.Framebuffer,
            (int)WebGlFramebufferAttachment.ColorAttachment0,
            (int)WebGlTextureType.Texture2D,
            (int)texture.TextureId,
            0);
        
        if (depthBuffer != null)
        {
            JSRuntime.FramebufferRenderbuffer(
                gl,
                (int)WebGlFramebufferTarget.Framebuffer,
                (int)WebGlFramebufferAttachment.DepthStencilAttachment,
                (int)WebGlRenderbufferTarget.Renderbuffer,
                (int)depthBuffer.RenderbufferId);
        }
        
        var status = JSRuntime.CheckFramebufferStatus(gl, (int)WebGlFramebufferTarget.Framebuffer);
        
        if (status != (int)WebGlFramebufferStatus.FramebufferComplete)
        {
            WebGlError error = (WebGlError)JSRuntime.GetError(gl);
            throw new Exception($"Framebuffer invalid: {status}, Error: {error}");
        }
    }

    public void Dispose()
    {
        texture.Dispose();
        JSRuntime.DeleteFramebuffer(Gl, (int)FramebufferId);
    }

    uint IWebGlTexture.TextureId => (uint)texture.TextureId;
}
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Silk.NET.OpenGL;

namespace Drawie.Interop.Avalonia.OpenGl;

public class OpenGlRenderApiResources : RenderApiResources
{
    public override ITexture Texture => renderTexture;

    private int fbo;
    internal OpenGlSwapchain Swapchain { get; }
    internal IGlContext Context => OpenGlInteropContext.Current.Context;

    private OpenGlTexture renderTexture;

    public OpenGlRenderApiResources(CompositionDrawingSurface surface, ICompositionGpuInterop gpuInterop) : base(
        surface, gpuInterop)
    {
        IOpenGlTextureSharingRenderInterfaceContextFeature sharingFeature =
            surface.Compositor.TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature))
                    .Result
                as IOpenGlTextureSharingRenderInterfaceContextFeature;
        Swapchain = new OpenGlSwapchain(OpenGlInteropContext.Current.Context, gpuInterop, surface, sharingFeature);

        using var ctx = Context.MakeCurrent();
        fbo = Context.GlInterface.GenFramebuffer();
    }

    public override async ValueTask DisposeAsync()
    {
        await Swapchain.DisposeAsync();
    }

    public override void CreateTemporalObjects(PixelSize size)
    {
        renderTexture?.Dispose();

        using var ctx = Context.MakeCurrent();
        GL gl = GL.GetApi(Context.GlInterface.GetProcAddress);
        renderTexture = new OpenGlTexture(gl, size.Width, size.Height);

        Context.GlInterface.Flush();
    }

    public override void Render(PixelSize size, Action renderAction)
    {
        using var ctx = Context.MakeCurrent();

        Context.GlInterface.BindFramebuffer((int)GLEnum.Framebuffer, fbo);

        using var _ = Swapchain.BeginDraw(size, out var texture);

        Context.GlInterface.FramebufferTexture2D((int)GLEnum.Framebuffer, (int)GLEnum.ColorAttachment0,
            (int)GLEnum.Texture2D, (int)texture.TextureId, 0);
        if (Context.GlInterface.CheckFramebufferStatus((int)GLEnum.Framebuffer) != (int)GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete");
        }

        Context.GlInterface.Flush();

        renderAction();
    }
}

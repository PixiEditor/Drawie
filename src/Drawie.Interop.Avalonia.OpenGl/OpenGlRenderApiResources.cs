using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Bridge;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Silk.NET.OpenGL;

namespace Drawie.Interop.Avalonia.OpenGl;

public class OpenGlRenderApiResources : RenderApiResources
{
    public override ITexture Texture => fboTexture;

    private int fbo;
    internal OpenGlSwapchain Swapchain { get; }
    internal IGlContext Context { get; }
    public override bool IsDisposed => isDisposed;

    private IGlContext globalContext;

    private OpenGlTexture fboTexture;

    private bool isDisposed;

    public OpenGlRenderApiResources(CompositionDrawingSurface surface, ICompositionGpuInterop gpuInterop) : base(
        surface, gpuInterop)
    {
        IOpenGlTextureSharingRenderInterfaceContextFeature sharingFeature =
            surface.Compositor.TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature))
                    .Result
                as IOpenGlTextureSharingRenderInterfaceContextFeature;

        Context = sharingFeature.CreateSharedContext();
        Swapchain = new OpenGlSwapchain(Context, gpuInterop, surface, sharingFeature);

        globalContext = OpenGlInteropContext.Current.Context;

        using (Context.MakeCurrent())
        {
            fbo = Context.GlInterface.GenFramebuffer();
        }

        fboTexture = new OpenGlTexture((uint)fbo, null);
    }

    public override async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        await Swapchain.DisposeAsync();
        if (fbo != 0)
        {
            using (Context.MakeCurrent())
            {
                Context.GlInterface.DeleteFramebuffer(fbo);
            }
        }
    }

    public override void CreateTemporalObjects(PixelSize size)
    {
    }

    public override void Render(PixelSize size, Action renderAction)
    {
        if (isDisposed)
            return;

        //var ctx = Context.MakeCurrent();

        Context.GlInterface.GetIntegerv((int)GLEnum.FramebufferBinding, out var oldFbo);
        Context.GlInterface.BindFramebuffer((int)GLEnum.Framebuffer, fbo);
        using (Swapchain.BeginDraw(size, out var texture))
        {
            Context.GlInterface.FramebufferTexture2D((int)GLEnum.Framebuffer, (int)GLEnum.ColorAttachment0,
                (int)GLEnum.Texture2D, (int)texture.TextureId, 0);
            if (Context.GlInterface.CheckFramebufferStatus((int)GLEnum.Framebuffer) !=
                (int)GLEnum.FramebufferComplete)
            {
                throw new Exception("Framebuffer is not complete");
            }

            //var disp = globalContext.MakeCurrent();
            renderAction();
            //disp.Dispose();

            //ctx = Context.MakeCurrent();
        }

        Context.GlInterface.BindFramebuffer((int)GLEnum.Framebuffer, oldFbo);
       // ctx.Dispose();
    }
}

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private VecI lastSize = VecI.Zero;

    public bool NeedsRedraw { get; private set; } = true;

    private DrawingSurface? framebuffer;

    /// <summary>
    ///     If true, intermediate surface will be used to render the frame. This is useful when dealing with non srgb surfaces.
    /// Enabling this will create display-optimized surface and use it for rendering, then draw it to the frame buffer.
    /// </summary>
    protected bool UseIntermediateSurface { get; set; } = true;

    protected override RenderApiResources? InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop, out string? createInfo)
    {
        try
        {
            createInfo = null;
            return IDrawieInteropContext.Current.CreateResources(compositionDrawingSurface, interop);
        }
        catch (Exception e)
        {
            createInfo = e.Message;
            return null;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();
        base.OnDetachedFromVisualTree(e);
        framebuffer?.Dispose();
        framebuffer = null;
    }

    protected override void FreeGraphicsResources()
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();

        framebuffer?.Dispose();
        framebuffer = null;
    }

    protected override void QueueFrameRequested()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height))
            return;

        NeedsRedraw = true;
        PrepareToDraw();
        RequestBlit();
        InvalidateVisual();

        /*DrawingBackendApi.Current.RenderingDispatcher.QueueRender(() =>
        {
            BeginDraw(new VecI((int)Bounds.Width, (int)Bounds.Height));
            Dispatcher.UIThread.Post(RequestBlit);
        });*/
    }

    protected virtual void PrepareToDraw()
    {
    }

    private IDisposable present;

    public void BeginDraw(VecI size)
    {
        if (!NeedsRedraw)
            return;

        if (resources is { IsDisposed: false })
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.X == 0 || size.Y == 0)
            {
                return;
            }

            if (lastSize != size)
            {
                resources.CreateTemporalObjects(size);
            }
        }
    }

    public abstract void Draw(ITexture resourcesTexture);

    protected override void RenderFrame(PixelSize pixelSize)
    {
        if (!NeedsRedraw)
            return;

        VecI size = new VecI(pixelSize.Width, pixelSize.Height);

        if (resources is { IsDisposed: false })
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.X == 0 || size.Y == 0)
            {
                return;
            }

            if (lastSize != size)
            {
                resources.CreateTemporalObjects(size);
            }
        }

        using var _ = resources.Render(size, () =>
        {
            Draw(resources.Texture);
        });

        NeedsRedraw = false;
        /*using var present = resources.Render(new VecI(pixelSize.Width, pixelSize.Height), () =>
        {
            framebuffer.Canvas.Clear();
            Draw(framebuffer);
            framebuffer.Flush();
        });*/
    }
}

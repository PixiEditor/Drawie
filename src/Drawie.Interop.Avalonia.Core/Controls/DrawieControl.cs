using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl, IDrawieControl
{
    private VecI lastSize = VecI.Zero;

    public bool NeedsRedraw { get; private set; } = true;

    private Texture? intermediateSurface;
    private DrawingSurface? framebuffer;

    private IDisposable? toPresent;

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
        intermediateSurface?.Dispose();
        intermediateSurface = null;

        framebuffer?.Dispose();
        framebuffer = null;
    }

    protected override void QueueFrameRequested()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height))
            return;
        PrepareToDraw();
        DrawingBackendApi.Current.RenderingDispatcher.QueueRender(() =>
        {
            BeginDraw(new VecI((int)Bounds.Width, (int)Bounds.Height));
            Dispatcher.UIThread.Post(RequestBlit);
        });
    }

    protected virtual void PrepareToDraw()
    {

    }

    public void BeginDraw(VecI size)
    {
        if (resources is { IsDisposed: false })
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.X == 0 || size.Y == 0)
            {
                return;
            }

            if (framebuffer == null || lastSize != size)
            {
                resources.CreateTemporalObjects(size);

                VecI sizeVec = new VecI(size.X, size.Y);

                framebuffer?.Dispose();

                framebuffer =
                    DrawingBackendApi.Current.CreateRenderSurface(sizeVec,
                        resources.Texture, SurfaceOrigin.BottomLeft);

                if (UseIntermediateSurface)
                {
                    intermediateSurface?.Dispose();
                    intermediateSurface = Texture.ForDisplay(sizeVec);
                }

                lastSize = size;
            }

            toPresent = resources.Render(size, () =>
            {
                framebuffer.Canvas.Clear();
                intermediateSurface?.DrawingSurface.Canvas.Clear();

                if (!UseIntermediateSurface)
                {
                    Draw(framebuffer);
                }
                else
                {
                    Draw(intermediateSurface.DrawingSurface);
                    framebuffer.Canvas.DrawSurface(intermediateSurface.DrawingSurface, 0, 0);
                }

                framebuffer.Flush();
            });
        }
    }

    public abstract void Draw(DrawingSurface surface);

    protected override void RenderFrame(PixelSize size)
    {
        toPresent?.Dispose();
        toPresent = null;
        NeedsRedraw = false;
    }
}

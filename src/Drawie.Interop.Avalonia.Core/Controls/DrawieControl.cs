using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private VecI lastSize = VecI.Zero;


    private ITexture backingFrontbufferTexture;
    private DrawingSurface? frontbuffer;

    private object backingLock = new object();
    private Texture? backbuffer;

    private DrawingSurface framebuffer;

    bool frontInUse = false;
    bool backInUse = false;

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
        frontbuffer?.Dispose();
        frontbuffer = null;

        backbuffer?.Dispose();
        backbuffer = null;
    }

    protected override void FreeGraphicsResources()
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();

        frontbuffer?.Dispose();
        frontbuffer = null;

        backbuffer?.Dispose();
        backbuffer = null;
    }

    protected override void QueueFrameRequested()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            PrepareToDraw();
        }
        else
        {
            Dispatcher.UIThread.Invoke(PrepareToDraw);
        }

        DrawingBackendApi.Current.RenderingDispatcher.QueueRender(() =>
        {
            RenderFrame(new PixelSize((int)Bounds.Width, (int)Bounds.Height));
        });
    }

    protected virtual void PrepareToDraw()
    {
    }

    public abstract void Draw(DrawingSurface texture);

    protected override void RenderFrame(PixelSize pixelSize)
    {
        VecI size = new VecI(pixelSize.Width, pixelSize.Height);

        if (backbuffer == null || backbuffer.IsDisposed || backbuffer.Size != size)
        {
            backbuffer?.Dispose();
            backbuffer = Texture.ForDisplay(size);
        }

        using (var ctx = IDrawieInteropContext.Current.EnsureContext())
        {
            backInUse = true;
            backbuffer.DrawingSurface.Canvas.Clear();
            Draw(backbuffer.DrawingSurface);
            backInUse = false;
        }

        DrawingBackendApi.Current.RenderingDispatcher.EnqueueUIUpdate(this, () =>
        {
            if (frontInUse) return;

            frontInUse = true;
            if (resources.Texture == null || lastSize != size)
            {
                resources.CreateTemporalObjects(size);
                lastSize = size;

                frontbuffer?.Dispose();
                backingFrontbufferTexture = resources.CreateSwapchainTexture(size);
                frontbuffer =
                    DrawingBackendApi.Current.CreateRenderSurface(size, backingFrontbufferTexture,
                        SurfaceOrigin.BottomLeft);
            }

            UpdateFrame();
            lock (backingLock)
            {
                /*using var p = resources.Render(size, () =>
                {
                    /*if (backingFrontbufferTexture is ISwapchainImage swapchain)
                    {
                        swapchain.Present().ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Console.WriteLine("Failed to present swapchain image: " + t.Exception?.Message);
                                frontInUse = false;
                            }
                            else
                            {
                                frontInUse = false;
                            }
                        });
                    }#1#

                });*/

                using var present = resources.Render(size, () =>
                {
                    if(backInUse) return;

                    lock (backbuffer.DrawingSurface)
                    {
                        frontbuffer.Canvas.Clear();
                        frontbuffer.Canvas.DrawSurface(backbuffer.DrawingSurface, 0, 0);
                        frontbuffer.Flush();

                        resources.Texture.BlitFrom(backingFrontbufferTexture);
                    }
                });

                frontInUse = false;
            }

            updateQueued = false;
        }, () => { });
    }

    public void WriteBackToFront()
    {
        lock (backingLock)
        {
            if (frontInUse || frontbuffer == null || backbuffer == null || backbuffer.IsDisposed)
            {
                return;
            }

            using (var ctx = IDrawieInteropContext.Current.EnsureContext())
            {
                frontbuffer.Canvas.Clear();
                frontbuffer.Canvas.DrawSurface(backbuffer.DrawingSurface, 0, 0);
            }
        }
    }

    protected FrameHandle ExportFrameForDisplay(ITexture backingTexture)
    {
        if (backingTexture is ISwapchainImage swapchainImage)
        {
            return swapchainImage.ExportFrame();
        }

        throw new NotSupportedException("Backing texture is not a swapchain image");
    }

    private void Present(FrameHandle frame)
    {
        try
        {
            var imported = resources.GpuInterop.ImportImage(frame.ImageHandle,
                new PlatformGraphicsExternalImageProperties()
                {
                    Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                    Width = frame.Size.X,
                    Height = frame.Size.Y,
                    MemorySize = frame.MemorySize,
                });

            var availableSemaphore = resources.GpuInterop.ImportSemaphore(frame.AvailableSemaphore);
            var renderCompletedSemaphore = resources.GpuInterop.ImportSemaphore(frame.RenderCompletedSemaphore);

            resources.Surface.UpdateWithSemaphoresAsync(imported, renderCompletedSemaphore, availableSemaphore)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Failed to present frame: " + t.Exception?.Message);
                    }

                    frontInUse = false;
                    Dispatcher.UIThread.Post(() =>
                    {
                        _ = imported.DisposeAsync();
                        _ = availableSemaphore.DisposeAsync();
                        _ = renderCompletedSemaphore.DisposeAsync();
                    });
                });
        }
        catch
        {
            Console.WriteLine("Failed to present frame");
            frontInUse = false;
        }
    }
}

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Rendering;
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
    private DrawingSurface? backbuffer;
    private ITexture backingBackbufferTexture;

    private SynchronizedRequest frameRequest;

    /// <summary>
    ///     If true, intermediate surface will be used to render the frame. This is useful when dealing with non srgb surfaces.
    /// Enabling this will create display-optimized surface and use it for rendering, then draw it to the frame buffer.
    /// </summary>
    protected bool UseIntermediateSurface { get; set; } = true;

    public DrawieControl()
    {
        frameRequest = new SynchronizedRequest(QueueRender,
            WriteBackToFront,
            () => Dispatcher.UIThread.Post(RequestCompositorUpdate));
    }

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

        frameRequest.QueueRequestBackbufferUpdate(new VecI((int)Bounds.Width, (int)Bounds.Height));
    }

    private void QueueRender(VecI size)
    {
        DrawingBackendApi.Current.RenderingDispatcher.QueueRender(() =>
        {
            frameRequest.SignalBackbufferUpdated(UpdateBackbuffer(size));
        });
    }

    protected virtual void PrepareToDraw()
    {
    }

    public abstract void Draw(DrawingSurface texture);

    protected FrameHandle UpdateBackbuffer(VecI size)
    {
        if (backbuffer == null || backbuffer.IsDisposed || backbuffer.DeviceClipBounds.Size != size)
        {
            backingBackbufferTexture = resources.CreateExportableTexture(size);
            backbuffer?.Dispose();
            backbuffer =
                DrawingBackendApi.Current.CreateRenderSurface(size, backingBackbufferTexture, SurfaceOrigin.BottomLeft);
        }

        using (var ctx = IDrawieInteropContext.Current.EnsureContext())
        {
            backbuffer.Canvas.Clear();
            Draw(backbuffer);
            backbuffer.Flush();
        }

        FrameHandle frameHandle = ExportFrameForDisplay(backingBackbufferTexture);

        return frameHandle;
    }

    protected override void OnCompositorRender(VecI size)
    {
        if (frameRequest.State == RenderState.Swapping)
        {
            return;
        }

        lock (backingLock)
        {
            Present(lastPresentedFrame);
        }

        updateQueued = false;
    }

    private FrameHandle lastPresentedFrame;
    public void WriteBackToFront(FrameHandle handle)
    {
        lock (backingLock)
        {
            if (backbuffer == null || backbuffer.IsDisposed)
            {
                return;
            }

            if (frontbuffer == null)
            {
                VecI size = new((int)Bounds.Width, (int)Bounds.Height);
                if (frontbuffer == null || frontbuffer.IsDisposed || lastSize != size)
                {
                    resources.CreateTemporalObjects(size);
                    lastSize = size;

                    frontbuffer?.Dispose();
                    backingFrontbufferTexture = resources.CreateSwapchainTexture(size);
                    frontbuffer =
                        DrawingBackendApi.Current.CreateRenderSurface(size, backingFrontbufferTexture,
                            SurfaceOrigin.BottomLeft);
                }
            }

            lastPresentedFrame = handle;
            frameRequest.SignalSwapFinished();
        }
    }

    protected FrameHandle ExportFrameForDisplay(ITexture backingTexture)
    {
        if (backingTexture is ISwapchainImage swapchainImage)
        {
            return swapchainImage.ExportFrame();
        }

        if (backingTexture is IExportable exportable)
        {
            return new FrameHandle()
            {
                ImageHandle = exportable.Export(),
                Size = backingTexture.Size,
                MemorySize = exportable.MemorySize,
                AvailableSemaphore = null,
                RenderCompletedSemaphore = null
            };
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
        }
    }
}

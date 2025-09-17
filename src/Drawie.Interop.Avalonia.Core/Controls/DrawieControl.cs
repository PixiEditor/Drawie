using System.Collections.Concurrent;
using System.Diagnostics;
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
    private object backingLock = new object();
    private DrawingSurface? backbuffer;
    private ITexture backingBackbufferTexture;

    private SynchronizedRequest frameRequest;
    private ConcurrentStack<PendingFrame> pendingFrames = new();
    private ConcurrentStack<IDisposable> pendingFrames2 = new();

    private ISwapchain swapchain;

    /// <summary>
    ///     If true, intermediate surface will be used to render the frame. This is useful when dealing with non srgb surfaces.
    /// Enabling this will create display-optimized surface and use it for rendering, then draw it to the frame buffer.
    /// </summary>
    protected bool UseIntermediateSurface { get; set; } = true;

    public DrawieControl()
    {
        frameRequest = new SynchronizedRequest(QueueRender,
            QueueWriteBackToFront,
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

        backbuffer?.Dispose();
        backbuffer = null;
    }

    protected override void FreeGraphicsResources()
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();

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
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            UpdateBackbuffer(size);
            frameRequest.SignalBackbufferUpdated();
        });
    }

    private void QueueWriteBackToFront(VecI size)
    {
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            WriteBackToFront(size);
        }, Priority.UI);
    }

    protected virtual void PrepareToDraw()
    {
    }

    public abstract void Draw(DrawingSurface texture);

    protected void UpdateBackbuffer(VecI size)
    {
        /*if (backbuffer == null || backbuffer.IsDisposed || backbuffer.DeviceClipBounds.Size != size)
        {
            lock (backingLock)
            {
                backingBackbufferTexture = resources.CreateExportableTexture(size);
            }
            backbuffer?.Dispose();
            backbuffer =
                DrawingBackendApi.Current.CreateRenderSurface(size, backingBackbufferTexture, SurfaceOrigin.BottomLeft);
        }*/

        if (resources.Texture == null || resources.Texture.Size != size)
        {
            backingBackbufferTexture = resources.CreateExportableTexture(size);
            backbuffer?.Dispose();
            backbuffer = DrawingBackendApi.Current.CreateRenderSurface(size, backingBackbufferTexture, SurfaceOrigin.BottomLeft);
        }

        using (var ctx = IDrawieInteropContext.Current.EnsureContext())
        {
            backbuffer.Canvas.Clear();
            Draw(backbuffer);
            backbuffer.Flush();
        }
    }

    protected override void OnCompositorRender(VecI size)
    {
        if (!frameRequest.TryStartPresenting())
        {
            return;
        }

        lock (backingLock)
        {
            /*if (pendingFrames.TryPop(out var frame))
            {
                Present(frame);
            }*/

            if (pendingFrames2.TryPop(out var frame2))
            {
                frame2.Dispose();
            }

            frameRequest.SignalPresentFinished();
        }
    }


    public void WriteBackToFront(VecI size)
    {
        lock (backingLock)
        {
            pendingFrames2.Push(resources.Render(size, backingBackbufferTexture));
            /*
            var exportTexture = resources.CreateExportableTexture(backingBackbufferTexture.Size);
            var semaphorePair = resources.CreateSemaphorePair();

            exportTexture.BlitFrom(backingBackbufferTexture, null, semaphorePair.AvailableSemaphore);

            FrameHandle frameHandle = ExportFrameForDisplay(exportTexture, semaphorePair, size);

            pendingFrames.Push(new PendingFrame()
            {
                Handle = frameHandle, NativeTexture = exportTexture, SemaphorePair = semaphorePair
            });
            */

            frameRequest.SignalSwapFinished();
        }
    }

    protected FrameHandle ExportFrameForDisplay(IExportableTexture exportableTexture, ISemaphorePair semaphorePair,
        VecI size)
    {
        var img = exportableTexture.Export();
        return new FrameHandle()
        {
            ImageHandle = img,
            Size = size,
            MemorySize = exportableTexture.MemorySize,
            RenderCompletedSemaphore = semaphorePair.Export(true),
            AvailableSemaphore = semaphorePair.Export(false),
        };
    }

    private void Present(PendingFrame frame)
    {
        try
        {
            frame.NativeTexture.PrepareForImport(frame.SemaphorePair.AvailableSemaphore, frame.SemaphorePair.RenderFinishedSemaphore);
            var imported = resources.GpuInterop.ImportImage(frame.Handle.ImageHandle,
                new PlatformGraphicsExternalImageProperties()
                {
                    Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                    Width = frame.Handle.Size.X,
                    Height = frame.Handle.Size.Y,
                    MemorySize = frame.Handle.MemorySize,
                });

            var renderCompletedSem = frame.Handle.RenderCompletedSemaphore != null
                ? resources.GpuInterop.ImportSemaphore(frame.Handle.RenderCompletedSemaphore)
                : null;

            var availableSem = frame.Handle.AvailableSemaphore != null
                ? resources.GpuInterop.ImportSemaphore(frame.Handle.AvailableSemaphore)
                : null;

            var task = resources.Surface.UpdateWithSemaphoresAsync(imported, renderCompletedSem,
                availableSem).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Failed to present frame: " + t.Exception?.Message);
                }

                /*Dispatcher.UIThread.Post(() =>
                {
                    imported.DisposeAsync();
                    renderCompletedSem.DisposeAsync();
                    availableSem.DisposeAsync();
                    FreePendingFrame(frame);
                }, DispatcherPriority.Background);*/
            });
        }
        catch
        {
            Console.WriteLine("Failed to present frame");
        }
    }

    private void FreePendingFrame(PendingFrame frame)
    {
        frame.NativeTexture.DisposeAsync();
        frame.SemaphorePair.Dispose();
    }
}

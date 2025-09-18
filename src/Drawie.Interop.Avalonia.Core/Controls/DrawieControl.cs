using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private object backingLock = new object();
    private DrawingSurface? backbuffer;

    private SynchronizedRequest frameRequest;

    private Frame lastFrame;
    private ConcurrentStack<Frame> pendingFrames = new ConcurrentStack<Frame>();

    /// <summary>
    ///     If true, intermediate surface will be used to render the frame. This is useful when dealing with non srgb surfaces.
    /// Enabling this will create display-optimized surface and use it for rendering, then draw it to the frame buffer.
    /// </summary>
    protected bool UseIntermediateSurface { get; set; } = true;

    public DrawieControl()
    {
        frameRequest = new SynchronizedRequest(QueueRender,
            QueueWriteBackToFront,
            QueueCompositorUpdate);
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

        if (Dispatcher.UIThread.CheckAccess())
        {
            RequestCompositorUpdate();
            return;
        }

        Dispatcher.UIThread.Post(RequestCompositorUpdate, DispatcherPriority.Render);
    }

    private void QueueWriteBackToFront(VecI size)
    {
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            WriteBackToFront(size);
        }, Priority.BackbufferUpdate);
    }

    private void QueueCompositorUpdate()
    {
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            Dispatcher.UIThread.Post(RequestCompositorUpdate, DispatcherPriority.Render);
        }, Priority.UI);
    }

    protected virtual void PrepareToDraw()
    {
    }

    public abstract void Draw(DrawingSurface texture);

    protected void UpdateBackbuffer(VecI size)
    {
        if (resources == null)
            return;

        if (resources.Texture == null || resources.Texture.Size != size)
        {
            resources.CreateTemporalObjects(size);

            backbuffer?.Dispose();
            backbuffer =
                DrawingBackendApi.Current.CreateRenderSurface(size, resources.Texture, SurfaceOrigin.BottomLeft);
        }

        using (var ctx = IDrawieInteropContext.Current.EnsureContext())
        {
            backbuffer.Canvas.Clear();
            Draw(backbuffer);
            backbuffer.Flush();
        }
    }

    public void WriteBackToFront(VecI size)
    {
        lock (backingLock)
        {
            pendingFrames.Push(resources?.Render(size, () => { }) ?? default);
        }

        frameRequest.SignalSwapFinished();
    }


    protected override void OnCompositorRender(VecI size)
    {
        lock (backingLock)
        {
            if (pendingFrames.TryPop(out var frame) && frame.PresentFrame != null)
            {
                if (size == frame.Size)
                {
                    frame.PresentFrame(size);
                    frame.ReturnFrame?.Dispose();
                }
                else
                {
                    frame.Texture?.DisposeAsync(); // Dont return to pool, size mismatch
                }
            }
        }
    }
}

using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Rendering;

public class SynchronizedRequest
{
    private VecI? queuedBackbufferUpdate;
    public RenderState State { get; private set; } = RenderState.Idle;
    private readonly object _lock = new();

    private Action<VecI> updateBackbuffer;
    private Action<FrameHandle> _blit;
    private Action requestRender;


    public SynchronizedRequest(Action<VecI> updateBackbuffer, Action<FrameHandle> blit, Action requestRender)
    {
        this.updateBackbuffer = updateBackbuffer;
        this._blit = blit;
        this.requestRender = requestRender;
    }

    public bool TryStartRendering()
    {
        lock (_lock)
        {
            if (State != RenderState.Idle) return false;
            State = RenderState.Rendering;
            return true;
        }
    }

    public bool TryStartSwapping()
    {
        lock (_lock)
        {
            if (State != RenderState.Rendering) return false;
            State = RenderState.Swapping;
            return true;
        }
    }

    public bool TryFinishSwapping()
    {
        lock (_lock)
        {
            if (State != RenderState.Swapping) return false;
            State = RenderState.Idle;
            return true;
        }
    }

    public void QueueRequestBackbufferUpdate(VecI vecI)
    {
        queuedBackbufferUpdate = vecI;
        if (State == RenderState.Idle)
        {
            TryStartRendering();
            updateBackbuffer(vecI);
        }
    }

    public void SignalBackbufferUpdated(FrameHandle frameHandle)
    {
        lock (_lock)
        {
            if (State != RenderState.Rendering) return;
            TryStartSwapping();
            _blit(frameHandle);
        }
    }

    public void SignalSwapFinished()
    {
        lock (_lock)
        {
            if (State != RenderState.Swapping) return;
            TryFinishSwapping();
            requestRender();
            if (queuedBackbufferUpdate.HasValue)
            {
                QueueRequestBackbufferUpdate(queuedBackbufferUpdate.Value);
            }
        }
    }
}

public enum RenderState
{
    Idle, // Backbuffer is idle, waiting to be rendered to
    Rendering, // Backbuffer is being rendered to, unable to swap with frontbuffer
    Swapping // Backbuffer is rendering onto frontbuffer. Backbuffer can't be rendered onto and frontbuffer can't be presented
}

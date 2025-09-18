using Avalonia.Media;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Rendering;

public class SynchronizedRequest
{
    private VecI? queuedBackbufferUpdate;
    private VecI? processingBackbufferUpdate;
    public RenderState State { get; private set; } = RenderState.Idle;
    private readonly object _lock = new();

    private Action<VecI> updateBackbuffer;
    private Action<VecI> _blit;
    private Action requestRender;


    public SynchronizedRequest(Action<VecI> updateBackbuffer, Action<VecI> blit, Action requestRender)
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

    /*public bool TryStartPresenting()
    {
        lock (_lock)
        {
            if (State != RenderState.Idle && State != RenderState.Presenting) return false;
            State = RenderState.Presenting;
            return true;
        }
    }*/

    public void QueueRequestBackbufferUpdate(VecI vecI)
    {
        queuedBackbufferUpdate = vecI;
        if (State is RenderState.Idle)
        {
            if (TryStartRendering())
            {
                queuedBackbufferUpdate = null;
                updateBackbuffer(vecI);
                processingBackbufferUpdate = vecI;
            }
        }
    }

    public void SignalBackbufferUpdated()
    {
        lock (_lock)
        {
            if (TryStartSwapping())
            {
                _blit(processingBackbufferUpdate!.Value);
            }
        }
    }

    public void SignalSwapFinished()
    {
        lock (_lock)
        {
            if (TryFinishSwapping())
            {
                processingBackbufferUpdate = null;
                requestRender();
                if (queuedBackbufferUpdate.HasValue)
                {
                    QueueRequestBackbufferUpdate(queuedBackbufferUpdate.Value);
                }
            }
        }
    }


    /*public void SignalPresentFinished()
    {
        lock (_lock)
        {
            if (State != RenderState.Presenting) return;
            State = RenderState.Idle;
            processingBackbufferUpdate = null;
            if (queuedBackbufferUpdate.HasValue)
            {
                QueueRequestBackbufferUpdate(queuedBackbufferUpdate.Value);
            }
        }
    }*/
}

public enum RenderState
{
    Idle, // Backbuffer is idle, waiting to be rendered to
    Rendering, // Backbuffer is being rendered to, unable to swap with frontbuffer
    Swapping, // Backbuffer is rendering onto frontbuffer. Backbuffer can't be rendered onto and frontbuffer can't be presented,
    /*
    Presenting // Frontbuffer is being presented, unable to swap
*/
}

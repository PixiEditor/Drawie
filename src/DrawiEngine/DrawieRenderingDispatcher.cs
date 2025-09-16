using Drawie.Backend.Core;
using Drawie.Backend.Core.Rendering;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();
    public Action<Action> ManualLoopTick { get; }

    public RenderThread RenderThread { get; }

    public DrawieRenderingDispatcher()
    {
        RenderThread = new RenderThread(null, Invoke);
    }

    public DrawieRenderingDispatcher(Action<Action> manualTick, Action<Action> mainThreadDispatcher)
    {
        ManualLoopTick = manualTick;
        RenderThread = new RenderThread(ManualLoopTick, mainThreadDispatcher);
    }

    public void QueueRender(Action renderAction)
    {
        RenderThread.EnqueueRender(renderAction);
    }

    public IDisposable EnsureContext()
    {
        return new EmptyDisposable();
    }

    public void StartRenderThread()
    {
        RenderThread.Start();
    }
}

public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

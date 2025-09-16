using Drawie.Backend.Core;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public RenderThread RenderThread { get; }

    public DrawieRenderingDispatcher()
    {
        RenderThread = new RenderThread(Invoke);
    }

    public DrawieRenderingDispatcher(Action<Action> mainThreadDispatcher)
    {
        RenderThread = new RenderThread(mainThreadDispatcher);
    }

    public void QueueRender(Action renderAction)
    {
        RenderThread.QueueRender(renderAction);
    }

    public IDisposable EnsureContext()
    {
        return new EmptyDisposable();
    }

    public void StartRenderThread()
    {
        RenderThread.Start();
    }

    public IDisposable PauseRenderThread()
    {
        return RenderThread.Pause();
    }

    public void EnqueueUIUpdate(Action update)
    {
        RenderThread.QueueUIUpdate(update);
    }
}

public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

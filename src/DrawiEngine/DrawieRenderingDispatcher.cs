using Drawie.Backend.Core;
using Drawie.Backend.Core.Rendering;

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

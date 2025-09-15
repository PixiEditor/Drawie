using Drawie.Backend.Core;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public RenderThread RenderThread { get; } = new();

    public DrawieRenderingDispatcher()
    {
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
}

public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

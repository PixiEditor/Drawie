using Drawie.Backend.Core;
using Drawie.Backend.Core.Rendering;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public RenderThread RenderThread { get; }

    public DrawieRenderingDispatcher()
    {
        RenderThread = new RenderThread();
    }

    public void Enqueue(Action renderAction)
    {
        RenderThread.Enqueue(renderAction, Priority.Render);
    }

    public void Enqueue(Action renderAction, Priority priority)
    {
        RenderThread.Enqueue(renderAction, priority);
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

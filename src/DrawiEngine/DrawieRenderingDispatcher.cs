using Drawie.Backend.Core;
using Drawie.Backend.Core.Rendering;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public RenderThread RenderThread { get; }

    public DrawieRenderingDispatcher(double targetFps = 60.0)
    {
        RenderThread = new RenderThread(targetFps);
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

    public async Task WaitForIdleAsync()
    {
        await RenderThread.WaitForIdleAsync();
    }
}

public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}


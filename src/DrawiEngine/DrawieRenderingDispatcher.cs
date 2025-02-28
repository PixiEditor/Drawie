using Drawie.Backend.Core;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public IDisposable EnsureContext()
    {
        return new EmptyDisposable();
    }
}

public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

using Drawie.Backend.Core;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => action();

    public async Task<TResult> InvokeAsync<TResult>(Func<TResult> function)
    {
        return await Task.Run(function);
    }

    public Task InvokeAsync(Action function)
    {
        return Task.Run(function);
    }

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

using Drawie.Backend.Core;

namespace DrawiEngine;

public class DrawieRenderingDispatcher : IRenderingDispatcher
{
    private bool renderApiReady = false;

    private List<Action> queuedActions = new List<Action>();

    public Action<Action> Invoke { get; }

    public DrawieRenderingDispatcher()
    {
        Invoke = OnInvoke;
    }

    private void OnInvoke(Action action)
    {
        if (renderApiReady)
        {
            action();
        }
        else
        {
            queuedActions.Add(action);
        }
    }

    void IRenderingDispatcher.RenderApiReady()
    {
        renderApiReady = true;

        foreach (var action in queuedActions)
        {
            action();
        }

        queuedActions.Clear();
    }

    public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
    {
        return await Task.Run(func);
    }

    public async Task<TResult> InvokeInBackgroundAsync<TResult>(Func<TResult> function)
    {
        return await Task.Run(function);
    }

    public Task InvokeInBackgroundAsync(Action function)
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
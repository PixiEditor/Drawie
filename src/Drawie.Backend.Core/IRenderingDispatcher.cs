namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> function);
    public Task InvokeAsync(Action function);
    public IDisposable EnsureContext();
}

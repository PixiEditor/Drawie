namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> function);
    public IDisposable EnsureContext();
}

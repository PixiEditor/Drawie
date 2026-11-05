namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    protected internal void RenderApiReady();
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> func);
    public Task<TResult> InvokeInBackgroundAsync<TResult>(Func<TResult> function);
    public Task InvokeInBackgroundAsync(Action function);
    public IDisposable EnsureContext();
}

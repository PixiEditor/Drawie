namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public void Enqueue(Action renderAction);
    public IDisposable EnsureContext();
    public void StartRenderThread();
    public Task WaitForIdleAsync();
}

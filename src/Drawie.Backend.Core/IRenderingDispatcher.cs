namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public void Enqueue(Action renderAction);
    public void Enqueue(Action renderAction, Priority priority);
    public IDisposable EnsureContext();
    public void StartRenderThread();
    public Task WaitForIdleAsync();
}

public enum Priority
{
    Render,
    BackbufferUpdate,
    UI
}

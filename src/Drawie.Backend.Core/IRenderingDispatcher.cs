namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public void Enqueue(Action renderAction);
    public void Enqueue(Action renderAction, Priority priority);
    public IDisposable EnsureContext();
    public void StartRenderThread();
}

public enum Priority
{
    Render,
    BackbufferUpdate,
    UI
}

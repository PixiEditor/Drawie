namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public void QueueRender(Action renderAction);
    public IDisposable EnsureContext();
    public void StartRenderThread();
    public IDisposable PauseRenderThread();
    public void EnqueueUIUpdate(Action update);
}

namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public void QueueRender(Action renderAction);
    public IDisposable EnsureContext();
    public void StartRenderThread();
    public void EnqueueUIUpdate(object requester, Action update, Action swapAction);
}

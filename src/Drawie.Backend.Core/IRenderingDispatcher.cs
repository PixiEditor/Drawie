namespace Drawie.Backend.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
    public IDisposable EnsureContext();
}

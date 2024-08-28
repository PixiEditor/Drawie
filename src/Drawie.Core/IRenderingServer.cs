namespace Drawie.Core;

public interface IRenderingServer
{
    public Action<Action> Invoke { get; }
}

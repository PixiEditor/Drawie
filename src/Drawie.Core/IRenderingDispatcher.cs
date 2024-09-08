namespace Drawie.Core;

public interface IRenderingDispatcher
{
    public Action<Action> Invoke { get; }
}

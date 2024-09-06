using Drawie.Core;

namespace DrawiEngine;

public class DrawieRenderingServer : IRenderingServer
{
    public Action<Action> Invoke { get; }
}
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Host.Input;

namespace Drawie.Host;

public interface IWindowingPlatform
{
    public IRenderApi RenderApi { get; }
    public IReadOnlyCollection<IHost> Windows { get; }
    public IHost CreateWindow(string name);
    public IHost CreateWindow(string name, VecI size);
}
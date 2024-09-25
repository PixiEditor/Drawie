using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Windowing.Input;

namespace Drawie.Windowing;

public interface IWindowingPlatform
{
    public IRenderApi RenderApi { get; }
    public IReadOnlyCollection<IWindow> Windows { get; }
    public IWindow CreateWindow(string name);
    public IWindow CreateWindow(string name, VecI size);
}
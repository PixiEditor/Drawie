using Drawie.RenderApi;
using PixiEditor.Numerics;

namespace Drawie.Windowing;

public interface IWindowingPlatform
{
    public IRenderApi RenderApi { get; }
    public IWindow CreateWindow(string name, VecI size);
}
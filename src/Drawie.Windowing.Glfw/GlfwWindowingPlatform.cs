using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Windowing;
using Drawie.Windowing.Input;
using Silk.NET.Input;
using IWindow = Drawie.Windowing.IWindow;

namespace Drawie.Silk;

public class GlfwWindowingPlatform : IWindowingPlatform
{
    private readonly List<IWindow> _windows = new();

    public IReadOnlyCollection<IWindow> Windows => _windows;
    public IRenderApi RenderApi { get; }

    public GlfwWindowingPlatform(IRenderApi renderApi)
    {
        RenderApi = renderApi;
    }

    public IWindow CreateWindow(string name)
    {
        return CreateWindow(name, VecI.Zero);
    }

    public IWindow CreateWindow(string name, VecI size)
    {
        GlfwWindow window = new(name, size, RenderApi.CreateWindowRenderApi());
        _windows.Add(window);
        return window;
    }

    public override string ToString()
    {
        return "Glfw";
    }
}
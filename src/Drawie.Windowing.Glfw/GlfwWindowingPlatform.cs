using Drawie.RenderApi;
using Drawie.Windowing;
using PixiEditor.Numerics;
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

    public IWindow CreateWindow(string name, VecI size)
    {
        GlfwWindow window = new(name, size, RenderApi.CreateWindowRenderApi());
        _windows.Add(window);
        return window;
    }
}
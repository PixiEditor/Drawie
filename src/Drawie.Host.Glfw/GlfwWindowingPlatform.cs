using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Host;
using Drawie.Host.Input;
using Silk.NET.Input;

namespace Drawie.Silk;

public class GlfwWindowingPlatform : IWindowingPlatform
{
    private readonly List<IHost> _windows = new();

    public IReadOnlyCollection<IHost> Windows => _windows;
    public IRenderApi RenderApi { get; }

    public GlfwWindowingPlatform(IRenderApi renderApi)
    {
        RenderApi = renderApi;
    }

    public IHost CreateWindow(string name)
    {
        return CreateWindow(name, VecI.Zero);
    }

    public IHost CreateWindow(string name, VecI size)
    {
        GlfwHost host = new(name, size, RenderApi.CreateWindowRenderApi());
        _windows.Add(host);
        return host;
    }

    public override string ToString()
    {
        return "Glfw";
    }
}
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Windowing.Browser;

public class BrowserWindowingPlatform(IRenderApi renderApi) : IWindowingPlatform
{
    public BrowserWindow Window { get; private set; }
    public IRenderApi RenderApi { get; } = renderApi;
    IReadOnlyCollection<IWindow> IWindowingPlatform.Windows => new IWindow[] { Window };
    public IWindow CreateWindow(string name)
    {
        return CreateWindow(name, VecI.Zero);
    }

    public IWindow CreateWindow(string name, VecI size)
    {
        if (Window != null)
        {
            throw new InvalidOperationException("Browser windowing platform can only have one window.");
        }

        BrowserWindow window = new BrowserWindow(RenderApi.CreateWindowRenderApi())
        {
            Name = name
        };
        
        Window = window;

        return window;
    }

    public override string ToString()
    {
        return "Browser";
    }
}
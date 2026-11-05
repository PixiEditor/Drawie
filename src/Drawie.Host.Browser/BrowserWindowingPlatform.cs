using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Host.Browser;

public class BrowserWindowingPlatform(IRenderApi renderApi) : IWindowingPlatform
{
    public BrowserWindow Window { get; private set; }
    public IRenderApi RenderApi { get; } = renderApi;
    IReadOnlyCollection<IHost> IWindowingPlatform.Windows => new IHost[] { Window };
    public IHost CreateWindow(string name)
    {
        return CreateWindow(name, VecI.Zero);
    }

    public IHost CreateWindow(string name, VecI size)
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
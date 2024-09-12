using Drawie.Backend.Core;
using Drawie.RenderApi;
using PixiEditor.Numerics;

namespace Drawie.Windowing.Browser;

public class BrowserWindow(IWindowRenderApi windowRenderApi) : IWindow
{
    public string Name
    {
        get => BrowserInterop.GetTitle();
        set => BrowserInterop.SetTitle(value);
    }
    
    public VecI Size { get; set; }

    public VecI UsableWindowSize => BrowserInterop.GetWindowSize();

    public IWindowRenderApi RenderApi { get; set; } = windowRenderApi;
    
    public event Action<double>? Update;
    public event Action<Texture, double>? Render;
    public void Initialize()
    {
        RenderApi.CreateInstance(null, UsableWindowSize);
    }

    public void Show()
    {
        
    }

    public void Close()
    {
        
    }
}
using Drawie.Backend.Core;
using Drawie.RenderApi;
using PixiEditor.Numerics;

namespace Drawie.Windowing.Browser;

public class BrowserWindow : IWindow
{
    public string Name
    {
        get => BrowserInterop.GetTitle();
        set => BrowserInterop.SetTitle(value);
    }
    
    public VecI Size { get; set; }
    
    public IWindowRenderApi RenderApi { get; set; }
    
    public event Action<double>? Update;
    public event Action<Texture, double>? Render;
    public void Initialize()
    {
        
    }

    public void Show()
    {
        
    }

    public void Close()
    {
        
    }
}
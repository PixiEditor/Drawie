using Drawie.Backend.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Rendering;
using Drawie.Windowing.Input;

namespace Drawie.Windowing;

public interface IWindow
{
    public string Name { get; set; }
    public VecI Size { get; } 
    
    public IWindowRenderApi RenderApi { get; set; }
    
    public InputController InputController { get; }
    public bool ShowOnTop { get; set; }

    public bool IsVisible { get; set;  }

    public event Action<double> Update;
    public event Action<TextureFramebuffer, double> Render;
    public event Action<VecI> Resize;
    
    public void Initialize();
    public void Show();
    public void Close();
    public void AddLayer(ILayer layer);
    public object NativeWindow { get; }
    public event Action Loaded;
    public void SubscribeToRender(string name, string renderAfter, Action<double> render);
}

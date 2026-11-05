using Drawie.Backend.Core;
using Drawie.Host.Input;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Rendering;

namespace Drawie.Host;

public interface IHost
{
    public string Name { get; set; }
    public VecI Size { get; } 
    
    public IHostViewRenderApi RenderApi { get; set; }
    
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
    public object Native { get; }
    public event Action Loaded;
    public void SubscribeToRender(string name, string renderAfter, Action<double> render);
    public void SubscribeToRenderContent(string name, string renderAfter, Action<TextureFramebuffer, double> render);
}

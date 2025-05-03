using Drawie.Backend.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
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
    public event Action<Texture, double> Render;
    
    public void Initialize();
    public void Show();
    public void Close();
}

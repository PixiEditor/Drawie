using Drawie.Backend.Core;
using Drawie.RenderApi;
using Drawie.Windowing.Input;
using PixiEditor.Numerics;

namespace Drawie.Windowing;

public interface IWindow
{
    public string Name { get; set; }
    public VecI Size { get; set; } 
    
    public IWindowRenderApi RenderApi { get; set; }
    
    public InputController InputController { get; }
    
    public event Action<double> Update;
    public event Action<Texture, double> Render;
    
    public void Initialize();
    public void Show();
    public void Close();
}
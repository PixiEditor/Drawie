using Drawie.RenderApi;
using PixiEditor.Numerics;

namespace Drawie.Windowing;

public interface IWindow
{
    public string Name { get; set; }
    public VecI Size { get; set; } 
    
    public IWindowRenderApi RenderApi { get; set; }
    
    public event Action<double> Update;
    public event Action<double> Render;
    
    public void Show();
    public void Close();
}
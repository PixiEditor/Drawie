namespace Drawie.Windowing;

public class RenderOrder
{
    public string Name { get; }
    public Action<double> Render { get; }
    
    public RenderOrder(string name, Action<double> render)
    {
        Name = name;
        Render = render;
    }
}
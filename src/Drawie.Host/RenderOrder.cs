using Drawie.Rendering;

namespace Drawie.Host;

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

public class RenderContentOrder
{
    public string Name { get; }
    public Action<TextureFramebuffer, double> Render { get; }
    
    public RenderContentOrder(string name, Action<TextureFramebuffer, double> render)
    {
        Name = name;
        Render = render;
    }
}

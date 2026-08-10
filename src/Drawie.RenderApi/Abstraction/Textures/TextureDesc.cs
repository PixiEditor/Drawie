namespace Drawie.RenderApi.Abstraction.Textures;

public struct TextureDesc
{
    public int Width { get; set; }
    public int Height { get; set; }

    public TextureFormat Format { get; set; }
    public DepthFormat Depth { get; set; }
}
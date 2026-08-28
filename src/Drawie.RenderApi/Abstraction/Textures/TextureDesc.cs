namespace Drawie.RenderApi.Abstraction.Textures;

public struct TextureDesc
{
    public int Width { get; set; }
    public int Height { get; set; }

    public TextureFormat Format { get; set; }
    public DepthFormat Depth { get; set; }
    public int Samples { get; set; }

    public TextureDesc()
    {
        Width = 0;
        Height = 0;
        Format = TextureFormat.RGBA8_Unorm;
        Depth = DepthFormat.NoDepth;
        Samples = 1;
    }
}
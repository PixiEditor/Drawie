namespace Drawie.RenderApi.Abstraction.Textures;

[Flags]
public enum TextureUsage
{
    Sampled,
    RenderTarget,
    DepthStencil
}
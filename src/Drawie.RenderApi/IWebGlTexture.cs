using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi;

public interface IWebGlTexture : ITexture
{
    public uint TextureId { get; }
}

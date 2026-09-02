using Drawie.Numerics;

namespace Drawie.RenderApi.Abstraction.Textures;

public interface ITexture
{
    public ulong TextureId { get; }
    public VecI Size { get; }
}
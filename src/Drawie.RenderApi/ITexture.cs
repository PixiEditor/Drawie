using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface ITexture
{
    public VecI Size { get; }
    public void BlitFrom(ITexture texture);
}

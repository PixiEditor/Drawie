using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi;

public interface IGraphicsContext
{
    public bool OwnsTexture(ITexture nativeTexture);
    public void MakeCurrent();
}
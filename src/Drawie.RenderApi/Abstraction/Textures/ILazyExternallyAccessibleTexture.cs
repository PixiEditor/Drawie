namespace Drawie.RenderApi.Abstraction.Textures;

public interface ILazyExternallyAccessibleTexture : ITexture
{
    void EnsureExternallyAccessible();
}
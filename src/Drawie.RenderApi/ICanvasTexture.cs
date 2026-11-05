using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi;

public interface ICanvasTexture : ITexture
{
    public string CanvasId { get; }
}
using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public IWindowRenderApi CreateWindowRenderApi();
    public ITexture CreateExportableTexture(VecI textureSize);
}

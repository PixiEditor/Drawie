using Drawie.RenderApi.Abstraction;

namespace Drawie.RenderApi;

public interface IRenderApi
{
    public IReadOnlyCollection<IHostViewRenderApi> WindowRenderApis { get; }
    public IGraphicsDevice GraphicsDevice { get; }
    public IHostViewRenderApi CreateWindowRenderApi();
}
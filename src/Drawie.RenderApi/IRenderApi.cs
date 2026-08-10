using Drawie.RenderApi.Abstraction;

namespace Drawie.RenderApi;

public interface IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public IGraphicsDevice GraphicsDevice { get; }
    public IWindowRenderApi CreateWindowRenderApi();
}
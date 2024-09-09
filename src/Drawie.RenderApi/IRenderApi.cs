namespace Drawie.RenderApi;

public interface IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public GraphicsApi GraphicsApi { get; }
    public IWindowRenderApi CreateWindowRenderApi();
}
namespace Drawie.RenderApi;

public interface IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public IWindowRenderApi CreateWindowRenderApi();
}
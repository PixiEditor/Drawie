namespace Drawie.RenderApi.WebGpu;

public class WebGpuRenderApi : IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public GraphicsApi GraphicsApi { get; } = GraphicsApi.WebGpu;
    public IWindowRenderApi CreateWindowRenderApi()
    {
        return new WebGpuWindowRenderApi();
    }
}
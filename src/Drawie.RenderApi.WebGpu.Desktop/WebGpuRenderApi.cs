namespace Drawie.RenderApi.WebGpu;

public class WebGpuRenderApi : IRenderApi
{
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public IWindowRenderApi CreateWindowRenderApi()
    {
        return new WebGpuWindowRenderApi();
    }
}
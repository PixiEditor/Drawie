namespace Drawie.RenderApi;

public class SoftwareRenderApi : IRenderApi
{
    private List<IWindowRenderApi> windowRenderApis = new List<IWindowRenderApi>();
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => windowRenderApis;

    public IWindowRenderApi CreateWindowRenderApi()
    {
        var windowApi = new SoftwareWindowRenderApi();
        windowRenderApis.Add(windowApi);
        return windowApi;
    }
}

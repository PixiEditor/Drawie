namespace Drawie.RenderApi.WebGl;

public class WebGlRenderApi : IWebGlRenderApi
{
    public IWebGlContext WebGlContext { get; private set; }
    public WebGlWindowRenderApi WindowRenderApi { get; private set; }
    
    IReadOnlyCollection<IWindowRenderApi> IRenderApi.WindowRenderApis => new List<IWindowRenderApi> { WindowRenderApi };
    
    public WebGlRenderApi()
    {
    }

    public IWindowRenderApi CreateWindowRenderApi()
    {
        if (WindowRenderApi != null)
        {
            throw new InvalidOperationException("Window render API was already created.");
        }

        WindowRenderApi = new WebGlWindowRenderApi();
        WebGlContext = new WebGlContext(WindowRenderApi);
        return WindowRenderApi;
    }
}

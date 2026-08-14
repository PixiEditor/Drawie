using Drawie.RenderApi.Abstraction;

namespace Drawie.RenderApi.WebGl;

public class WebGlRenderApi : IWebGlRenderApi
{
    public IWebGlContext WebGlContext { get; private set; }
    public WebGlHostViewRenderApi HostViewRenderApi { get; private set; }
    
    IReadOnlyCollection<IHostViewRenderApi> IRenderApi.WindowRenderApis => new List<IHostViewRenderApi> { HostViewRenderApi };
    public IGraphicsDevice GraphicsDevice { get; private set; }

    public WebGlRenderApi()
    {
    }

    public IHostViewRenderApi CreateWindowRenderApi()
    {
        if (HostViewRenderApi != null)
        {
            throw new InvalidOperationException("Window render API was already created.");
        } 

        HostViewRenderApi = new WebGlHostViewRenderApi();
        
        if (GraphicsDevice == null)
        {
            HostViewRenderApi.InstanceCreated += () => CreateGraphicsDevice(HostViewRenderApi.gl);
        }
        
        WebGlContext = new WebGlContext(HostViewRenderApi);
        return HostViewRenderApi;
    }
    
    private void CreateGraphicsDevice(int context)
    {
        GraphicsDevice = new WebGlGraphicsDevice(context);
    }
}

namespace Drawie.RenderApi.OpenGL;

public class OpenGlRenderApi : IOpenGlRenderApi
{
    private List<OpenGlWindowRenderApi> windowRenderApis = new List<OpenGlWindowRenderApi>();

    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => windowRenderApis;

    public IOpenGlContext OpenGlContext
    {
        get
        {
            if (context == null)
            {
                context = new OpenGlContext(s =>
                    windowRenderApis[0].Context.TryGetProcAddress(s, out IntPtr ptr) ? ptr : IntPtr.Zero, false);
            }

            return context;
        }
    }


    private IOpenGlContext? context;

    public OpenGlRenderApi()
    {
    }

    public OpenGlRenderApi(IOpenGlContext context)
    {
        this.context = context;
    }

    public IWindowRenderApi CreateWindowRenderApi()
    {
        OpenGlWindowRenderApi renderApi = new OpenGlWindowRenderApi();
        windowRenderApis.Add(renderApi);

        return renderApi;
    }
}

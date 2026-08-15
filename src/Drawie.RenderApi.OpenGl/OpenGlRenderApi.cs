using Drawie.RenderApi.Abstraction;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlRenderApi : IOpenGlRenderApi
{
    private List<OpenGlHostViewRenderApi> windowRenderApis = new List<OpenGlHostViewRenderApi>();

    public IReadOnlyCollection<IHostViewRenderApi> WindowRenderApis => windowRenderApis;
    public IGraphicsDevice GraphicsDevice { get; private set; }

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
        CreateGraphicsDevice(context);
    }

    public IHostViewRenderApi CreateWindowRenderApi()
    {
        OpenGlHostViewRenderApi renderApi = new OpenGlHostViewRenderApi();
        windowRenderApis.Add(renderApi);

        if (GraphicsDevice == null)
        {
            CreateGraphicsDevice(OpenGlContext);
        }

        return renderApi;
    }

    private void CreateGraphicsDevice(IOpenGlContext context)
    {
        GraphicsDevice = new OpenGlDevice(context);
    }
}

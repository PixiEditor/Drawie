using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Host;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Rendering;

namespace Drawie.Layer.UI.MiniUi;

public class MiniUILayer : ILayer
{
    private Action<double> render;
    private MiniUiContext context = new MiniUiContext();
    
    private IHostViewRenderApi renderApi;
    
    private Texture renderTexture;
    private IHost host;
    
    public MiniUILayer(Action<double> render)
    {
        this.render = render;
    }
    
    public bool IsRenderApiSupported(IHostViewRenderApi api)
    {
        return true;
    }

    public void Initialize(IHost host)
    {
        this.host = host;
        renderApi = host.RenderApi;
        host.SubscribeToRenderContent("MiniUi.Render", "RenderContent", HostOnRender);
        host.Update += HostOnUpdate;
    }

    private void HostOnUpdate(double obj)
    {
        context.Update(host.InputController);
    }

    private void HostOnRender(TextureFramebuffer textureFramebuffer, double deltaTime)
    {
        using var ctx = context.MakeActive(textureFramebuffer);
        render?.Invoke(deltaTime);
    }
}
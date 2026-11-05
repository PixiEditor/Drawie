using Drawie.RenderApi;
using Drawie.Rendering;
using Drawie.Host;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Drawie.Layer.UI.ImGui;

public class ImGuiLayer : ILayer
{
    public Action<double> Render { get; set; }
    private IHost _host;
    private IOpenGlHostViewRenderApi renderApi;

    private ImGuiController _controller;

    public ImGuiLayer(Action<double> render)
    {
        Render = render;
    }

    public bool IsRenderApiSupported(IHostViewRenderApi api)
    {
        return api is IOpenGlHostViewRenderApi;
    }

    public void Initialize(IHost host)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if (host.RenderApi is not IOpenGlHostViewRenderApi openGlRenderApi)
        {
            throw new InvalidOperationException("ImGui only supports OpenGL render APIs.");
        }

        renderApi = openGlRenderApi;
        this._host = host;

        OnLoaded();
        host.SubscribeToRender("ImGui.Update", "Init", OnEarlyRender);
        host.SubscribeToRender("ImGui.Render", "RenderContent", OnRender);
    }

    private void OnRender(double dt)
    {
        Render?.Invoke(dt);
        _controller.Render();
    }

    private void OnEarlyRender(double dt)
    {
        _controller.Update((float)dt);
    }

    private void OnLoaded()
    {
        var gl = new GL(new LamdaNativeContext(renderApi.GetGlInterface()));
        
        _controller = new ImGuiController(gl,
            _host.Native as IView,
            _host.InputController.NativeInputController as IInputContext);
    }
}
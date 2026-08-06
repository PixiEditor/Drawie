using Drawie.RenderApi;
using Drawie.Rendering;
using Drawie.Windowing;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using IWindow = Drawie.Windowing.IWindow;

namespace Drawie.Layer.UI.ImGui;

public class ImGuiLayer : ILayer
{
    public Action<double> Render { get; set; }
    private IWindow window;
    private IOpenGlWindowRenderApi renderApi;

    private ImGuiController _controller;

    public ImGuiLayer(Action<double> render)
    {
        Render = render;
    }
    
    public void Initialize(IWindow window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        if (window.RenderApi is not IOpenGlWindowRenderApi openGlRenderApi)
        {
            throw new InvalidOperationException("ImGui only supports OpenGL render APIs.");
        }

        renderApi = openGlRenderApi;
        this.window = window;
        
        OnLoaded();
        window.SubscribeToRender("ImGui.Update", "Init", OnEarlyRender);
        window.SubscribeToRender("ImGui.Render", "Render", OnRender);
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
        _controller =
            new ImGuiController(new GL(new LamdaNativeContext(renderApi.GetGlInterface())),
                window.NativeWindow as IView, window.InputController.NativeInputController as IInputContext);
    }
}
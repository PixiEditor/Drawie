using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Rendering;
using Drawie.Windowing.Browser.Input;
using Drawie.Windowing.Input;

namespace Drawie.Windowing.Browser;

public class BrowserWindow(IWindowRenderApi windowRenderApi) : IWindow
{
    public string Name
    {
        get => BrowserInterop.GetTitle();
        set => BrowserInterop.SetTitle(value);
    }

    public VecI Size
    {
        get => UsableWindowSize;
    }

    public VecI UsableWindowSize => BrowserInterop.GetWindowSize();

    public IWindowRenderApi RenderApi { get; set; } = windowRenderApi;

    public InputController InputController { get; private set; }

    public bool ShowOnTop
    {
        get => false;
        set { }
    }

    public bool IsVisible
    {
        get => true;
        set
        {
            throw new NotSupportedException("Browser windows cannot be hidden.");
        }
    }

    public event Action<double>? Update;
    public event Action<TextureFramebuffer, double>? Render;

    private Texture renderTexture;
    private GraphicsStore store;

    public void Initialize()
    {
        RenderApi.CreateInstance(null, UsableWindowSize);
        RenderApi.FramebufferResized += FramebufferResized;

        InputController = new InputController(new [] { new BrowserKeyboard() }, []);
        store = new GraphicsStore(RenderApi.GraphicsContext);
    }

    private void FramebufferResized()
    {
        store.Dispose();
        renderTexture = CreateRenderTexture();
    }

    public void Show()
    {
        renderTexture = CreateRenderTexture();
        OnRender(0);
        BrowserInterop.SubscribeWindowResize(OnWindowResized);
    }

    private void OnRender(double dt)
    {
        double deltaTime = dt / 1000.0;
        Update?.Invoke(deltaTime);
        RenderApi.PrepareTextureToWrite();
        RenderingContext ctx = new RenderingContext();
        using var renderingScope = ctx.Open();
        using var fbo = ctx.Edit(renderTexture);
        fbo.Clear();
        Render?.Invoke(fbo, dt);
        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    public void Close()
    {
    }

    private void OnWindowResized(int width, int height)
    {
        RenderApi?.UpdateFramebufferSize(width, height);
        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    private Texture CreateRenderTexture()
    {
        return store.CreateNativeRenderSurface(UsableWindowSize, RenderApi.RenderTexture, SurfaceOrigin.BottomLeft);
    }
}

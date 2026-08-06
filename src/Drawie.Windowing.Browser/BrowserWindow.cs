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
    private VecI? pendingResize = null;
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
        pendingResize = UsableWindowSize;
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
        if (pendingResize.HasValue)
        {
            if (pendingResize.Value != renderTexture.Size)
            {
                var newRenderTexture = CreateRenderTexture();
                
                var oldTexture = renderTexture;
                renderTexture = newRenderTexture;
                
                store.DisposeTexture(oldTexture);
            }

            pendingResize = null;
        }

        RenderApi.PrepareTextureToWrite();
        RenderingContext ctx = new RenderingContext(RenderApi.GraphicsContext);
        var renderingScope = ctx.Open();
        var fbo = ctx.Edit(renderTexture);
        fbo.Clear();
        Render?.Invoke(fbo, dt);
        fbo.Dispose();
        renderingScope.Dispose();
        ctx.Dispose();
        RenderApi.Render(dt);
        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    public void Close()
    {
    }

    private void OnWindowResized(int width, int height)
    {
        RenderApi?.UpdateFramebufferSize(width, height);
    }

    private Texture CreateRenderTexture()
    {
        return store.CreateNativeRenderSurface(UsableWindowSize, RenderApi.RenderTexture, SurfaceOrigin.BottomLeft);
    }
}

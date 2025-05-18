using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
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
    public event Action<Texture, double>? Render;

    private Texture renderTexture;

    public void Initialize()
    {
        RenderApi.CreateInstance(null, UsableWindowSize);
        RenderApi.FramebufferResized += FramebufferResized;

        InputController = new InputController(new [] { new BrowserKeyboard() }, []);
    }

    private void FramebufferResized()
    {
        renderTexture?.Dispose();
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
        renderTexture.DrawingSurface?.Canvas.Clear();
        Render?.Invoke(renderTexture, deltaTime);
        renderTexture.DrawingSurface?.Flush();
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
        var drawingSurface =
            DrawingBackendApi.Current.CreateRenderSurface(UsableWindowSize, RenderApi.RenderTexture,
                SurfaceOrigin.BottomLeft);
        return Texture.FromExisting(drawingSurface);
    }
}

using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Web.Common;
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

    public object NativeWindow => canvas;
    public event Action? Loaded;

    private Texture renderTexture;
    private VecI? pendingResize = null;
    private GraphicsStore store;
    private HtmlCanvas canvas;

    private List<ILayer> layers = new List<ILayer>();
    private List<RenderOrder> renderStack = new List<RenderOrder>();

    public void Initialize()
    {
        JSRuntime.InterceptGLObject();
        var canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        canvas = canvasObject;
        RenderApi.CreateInstance(canvasObject, UsableWindowSize);
        RenderApi.FramebufferResized += FramebufferResized;

        InputController = new InputController(new [] { new BrowserKeyboard() }, [], null);
        store = new GraphicsStore(RenderApi.GraphicsContext);
        
        renderStack.Add(new RenderOrder("Init", _ => { }));
        renderStack.Add(new RenderOrder("Render", RenderContent));
        renderStack.Add(new RenderOrder("RenderApi", RenderApi.Render));
    }
    
    public void AddLayer(ILayer layer)
    {
        layers.Add(layer);
    }

    public void SubscribeToRender(string name, string renderAfter, Action<double> render)
    {
        var foundRenderAfter = renderStack.FindIndex(r => r.Name == renderAfter);
        if (foundRenderAfter != -1)
        {
            renderStack.Insert(foundRenderAfter + 1, new RenderOrder(name, render));
        }
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
        Loaded?.Invoke();

        foreach (var layer in layers)
        {
            layer.Initialize(this);
        }
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

        foreach (var layer in renderStack)
        {
            layer.Render(dt);
        }

        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    private void RenderContent(double dt)
    {
        RenderApi.PrepareTextureToWrite();
        RenderingContext ctx = new RenderingContext(RenderApi.GraphicsContext);
        var renderingScope = ctx.Open();
        var fbo = ctx.Edit(renderTexture);
        fbo.Clear();
        Render?.Invoke(fbo, dt);
        fbo.Dispose();
        renderingScope.Dispose();
        ctx.Dispose();
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

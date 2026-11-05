using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Host;
using Drawie.Host.Browser.Input;
using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Web.Common;
using Drawie.Rendering;
using Drawie.Host.Input;

namespace Drawie.Host.Browser;

public class BrowserWindow(IHostViewRenderApi hostViewRenderApi) : IHost
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

    public IHostViewRenderApi RenderApi { get; set; } = hostViewRenderApi;

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
    public event Action<VecI>? Resize;

    public object Native => canvas;
    public event Action? Loaded;

    private Texture renderTexture;
    private VecI? pendingResize = null;
    private HtmlCanvas canvas;

    private List<ILayer> layers = new List<ILayer>();
    private List<RenderOrder> renderStack = new List<RenderOrder>();
    private List<RenderContentOrder> renderContentOrder = new List<RenderContentOrder>();

    public void Initialize()
    {
        JSRuntime.InterceptGLObject();
        var canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        canvas = canvasObject;
        RenderApi.CreateInstance(canvasObject, UsableWindowSize);
        RenderApi.FramebufferResized += FramebufferResized;

        InputController = new InputController(new [] { new BrowserKeyboard() }, [new BrowserPointer()], null);
        
        renderStack.Add(new RenderOrder("Init", _ => { }));
        renderStack.Add(new RenderOrder("RenderContent", RenderContent));
        renderStack.Add(new RenderOrder("RenderApi", RenderApi.Render));
        
        renderContentOrder.Add(new RenderContentOrder("Init", (_, _) => {}));
        renderContentOrder.Add(new RenderContentOrder("RenderContent", OnRenderContentDefault));
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

    public void SubscribeToRenderContent(string name, string renderAfter, Action<TextureFramebuffer, double> render)
    {
        var foundRenderAfter = renderContentOrder.FindIndex(r => r.Name == renderAfter);
        if (foundRenderAfter != -1)
        {
            renderContentOrder.Insert(foundRenderAfter + 1, new RenderContentOrder(name, render));
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
                
                oldTexture.Dispose();
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

        foreach (var layer in renderContentOrder)
        {
            layer.Render(fbo, dt);
        }
        
        fbo.Dispose();
        renderingScope.Dispose();
        ctx.Dispose();
    }
    
    
    private void OnRenderContentDefault(TextureFramebuffer fbo, double dt)
    {
        Render?.Invoke(fbo, dt);
    }

    public void Close()
    {
    }
    
    private void OnWindowResized(int width, int height)
    {
        RenderApi?.UpdateFramebufferSize(width, height);
        Resize?.Invoke(new VecI(width, height));
    }

    private Texture CreateRenderTexture()
    {
        return new Texture(Texture.FromExisting(DrawingBackendApi.Current.CreateRenderSurface(UsableWindowSize, RenderApi.RenderTexture, SurfaceOrigin.BottomLeft)));
    }
}

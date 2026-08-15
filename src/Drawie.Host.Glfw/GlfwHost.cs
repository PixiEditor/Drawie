using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Rendering;
using Drawie.Silk.Extensions;
using Drawie.Silk.Input;
using Drawie.Skia;
using Drawie.Host;
using Drawie.Host.Input;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;
using IKeyboard = Drawie.Host.Input.IKeyboard;
using IWindow = Silk.NET.Windowing.IWindow;

namespace Drawie.Silk;

public class GlfwHost : Drawie.Host.IHost
{
    private IWindow? window;
    private bool isRunning;

    public string Name
    {
        get => window?.Title ?? string.Empty;
        set
        {
            if (window != null) window.Title = value;
        }
    }

    public VecI Size
    {
        get => window?.Size.ToVecI() ?? VecI.Zero;
        set
        {
            if (window != null) window.Size = value.ToVector2DInt();
        }
    }

    public bool IsVisible
    {
        get => window?.IsVisible ?? false;
        set
        {
            if (window != null) window.IsVisible = value;
        }
    }

    public IHostViewRenderApi RenderApi { get; set; }

    public InputController InputController { get; private set; }

    public bool ShowOnTop
    {
        get => window?.TopMost ?? false;
        set => window?.TopMost = value;
    }

    public object Native => window;
    public event Action? Loaded;

    public event Action<double> Update;
    public event Action<TextureFramebuffer, double> Render;
    public event Action<VecI>? Resize;

    private SKSurface? surface;
    private Texture renderTexture;
    private bool initialized;

    private List<ILayer> layers = new List<ILayer>();
    private List<RenderOrder> renderStack = new List<RenderOrder>();
    private List<RenderContentOrder> renderContentStack = new List<RenderContentOrder>();

    public GlfwHost(string name, VecI size, IHostViewRenderApi renderApi)
    {
        window = Window.Create(WindowOptions.Default with
        {
            Title = name,
            Size = size.ToVector2DInt(),
            API = renderApi is IVulkanHostViewRenderApi ? GraphicsAPI.DefaultVulkan : GraphicsAPI.Default,
            Samples = 4
        });

        window.Load += () => Loaded?.Invoke();
        RenderApi = renderApi;

        renderStack.Add(new RenderOrder("Init", _ => { }));
        renderStack.Add(new RenderOrder("Render", RenderContent));
        renderStack.Add(new RenderOrder("RenderApi", RenderApi.Render));

        renderContentStack.Add(new RenderContentOrder("Init", (_, _) => { }));
        renderContentStack.Add(new RenderContentOrder("RenderContent", DefaultRenderContent));
    }

    private void DefaultRenderContent(TextureFramebuffer fbo, double dt)
    {
        Render?.Invoke(fbo, dt);
    }


    public void Initialize()
    {
        if (initialized) return;

        window.Initialize();
        InitInput();

        if (RenderApi is IVulkanHostViewRenderApi)
        {
            if (window.VkSurface == null)
            {
                throw new Exception("Vulkan surface is null");
            }

            GlfwVulkanContextInfo info = new GlfwVulkanContextInfo(window.VkSurface!);
            RenderApi.CreateInstance(info, window.Size.ToVecI());
        }
        else if (RenderApi is IOpenGlHostViewRenderApi)
        {
            RenderApi.CreateInstance(window.GLContext, window.Size.ToVecI());
        }
        else
        {
            RenderApi.CreateInstance(window.Native, window.Size.ToVecI());
        }

        for (int i = 0; i < layers.Count; i++)
        {
            if (!layers[i].IsRenderApiSupported(RenderApi))
            {
                Console.WriteLine($"Layer {layers[i]} is not supported on this render api. Skipping...");
                layers.RemoveAt(i);
                i--;
            }
        }

        foreach (var layer in layers)
        {
            layer.Initialize(this);
        }

        initialized = true;
    }

    private void InitInput()
    {
        var input = window.CreateInput();

        GlfwKeyboard[] keyboards = new GlfwKeyboard[input.Keyboards.Count];
        for (var i = 0; i < input.Keyboards.Count; i++)
        {
            var key = input.Keyboards[i];
            var keyboard = new GlfwKeyboard(key);

            keyboards[i] = keyboard;
        }

        GlfwPointer[] pointers = new GlfwPointer[input.Mice.Count];
        for (var i = 0; i < input.Mice.Count; i++)
        {
            var pointer = input.Mice[i];
            pointers[i] = new GlfwPointer(pointer);
        }

        InputController = new InputController(keyboards, pointers, input);
    }

    public void Show()
    {
        if (!isRunning)
        {
            if (!initialized)
            {
                Initialize();
            }

            window.FramebufferResize += WindowOnFramebufferResize;
            RenderApi.FramebufferResized += RenderApiOnFramebufferResized;

            CreateRenderTarget(window.FramebufferSize.ToVecI(), RenderApi.RenderTexture);

            window.Render += OnRender;

            window.Update += OnUpdate;
            isRunning = true;
            window.Run();
        }
    }

    private void RenderApiOnFramebufferResized()
    {
        renderTexture.Dispose();
        surface = null!;

        CreateRenderTarget(window!.FramebufferSize.ToVecI(), RenderApi.RenderTexture);
    }

    private void CreateRenderTarget(VecI size, ITexture nativeRenderTexture)
    {
        renderTexture = new Texture(NativeTexture.FromExisting(DrawingBackendApi.Current.CreateRenderSurface(size,
            nativeRenderTexture,
            RenderApi is IVulkanHostViewRenderApi ? SurfaceOrigin.TopLeft : SurfaceOrigin.BottomLeft)));
    }

    private void WindowOnFramebufferResize(Vector2D<int> newSize)
    {
        RenderApi.UpdateFramebufferSize(newSize.X, newSize.Y);
        Resize?.Invoke(newSize.ToVecI());
    }

    private void OnUpdate(double dt)
    {
        Update?.Invoke(dt);
    }

    private void OnRender(double dt)
    {
        foreach (var layer in renderStack)
        {
            layer.Render(dt);
        }
    }

    private void RenderContent(double dt)
    {
        RenderApi.PrepareTextureToWrite();
        RenderingContext ctx = new RenderingContext(RenderApi.GraphicsContext);
        using var renderingScope = ctx.Open();
        using var fbo = ctx.Edit(renderTexture);
        fbo.Clear();
        foreach (var layer in renderContentStack)
        {
            layer.Render(fbo, dt);
        }
    }

    public void Close()
    {
        window.Update -= OnUpdate;
        window.Render -= OnRender;
        RenderApi.DestroyInstance();

        window?.Close();
        window?.Dispose();
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
        else
        {
            renderStack.Add(new RenderOrder(name, render));
        }
    }

    public void SubscribeToRenderContent(string name, string renderAfter, Action<TextureFramebuffer, double> render)
    {
        var foundRenderAfter = renderContentStack.FindIndex(r => r.Name == renderAfter);
        if (foundRenderAfter != -1)
        {
            renderContentStack.Insert(foundRenderAfter + 1, new RenderContentOrder(name, render));
        }
        else
        {
            renderContentStack.Add(new RenderContentOrder(name, render));
        }
    }
}
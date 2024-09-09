using Drawie.Core;
using Drawie.Core.Bridge;
using Drawie.Core.ColorsImpl;
using Drawie.RenderApi;
using Drawie.Silk.Extensions;
using PixiEditor.Numerics;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace Drawie.Silk;

public class GlfwWindow : Drawie.Windowing.IWindow
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

    public IWindowRenderApi RenderApi { get; set; }

    public event Action<double> Update;
    public event Action<Texture, double> Render;

    private SKSurface? surface;
    private Texture renderTexture;
    private GRContext context;
    private bool initialized;

    public GlfwWindow(string name, VecI size, IWindowRenderApi renderApi)
    {
        window = Window.Create(WindowOptions.Default with
        {
            Title = name,
            Size = size.ToVector2DInt(),
            API = renderApi.GraphicsApi.ToSilkGraphicsApi()
        });

        RenderApi = renderApi;
    }

    public void Initialize()
    {
        if (initialized) return;
        
        window.Initialize();

        if (RenderApi.GraphicsApi == GraphicsApi.Vulkan)
            RenderApi.CreateInstance(window.VkSurface, window.Size.ToVecI());
        else
            throw new NotSupportedException($"Provided graphics API '{RenderApi.GraphicsApi}' is not supported.");
        
        initialized = true;
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

            CreateRenderTarget(window.FramebufferSize.ToVecI(), RenderApi);

            window.Render += OnRender;
            window.Render += RenderApi.Render;

            window.Update += OnUpdate;
            isRunning = true;
            window.Run();
        }
    }

    private void RenderApiOnFramebufferResized()
    {
        renderTexture.Dispose();
        renderTexture = null!;
        surface = null!;

        CreateRenderTarget(window!.FramebufferSize.ToVecI(), RenderApi);
    }

    private void CreateRenderTarget(VecI size, IWindowRenderApi renderApi)
    {
        var drawingSurface = DrawingBackendApi.Current.CreateRenderSurface(size, renderApi);
        renderTexture = Texture.FromExisting(drawingSurface);
    }

    private void WindowOnFramebufferResize(Vector2D<int> newSize)
    {
        RenderApi.UpdateFramebufferSize(newSize.X, newSize.Y);
    }

    private void OnUpdate(double dt)
    {
        Update?.Invoke(dt);
    }

    private void OnRender(double dt)
    {
        RenderApi.PrepareTextureToWrite();
        renderTexture.DrawingSurface?.Canvas.Clear();
        Render?.Invoke(renderTexture, dt);
        renderTexture.DrawingSurface?.Flush();
    }

    public void Close()
    {
        window.Update -= OnUpdate;
        window.Render -= OnRender;
        renderTexture.Dispose();
        RenderApi.DestroyInstance();

        window?.Close();
        window?.Dispose();
    }
}
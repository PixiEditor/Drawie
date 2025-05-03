using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Silk.Extensions;
using Drawie.Silk.Input;
using Drawie.Skia;
using Drawie.Windowing.Input;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;
using IKeyboard = Drawie.Windowing.Input.IKeyboard;

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

    public bool IsVisible
    {
        get => window?.IsVisible ?? false;
        set
        {
            if (window != null) window.IsVisible = value;
        }
    }

    public IWindowRenderApi RenderApi { get; set; }

    public InputController InputController { get; private set; }

    public bool ShowOnTop
    {
        get => window?.TopMost ?? false;
        set
        {
            if (window != null) window.TopMost = value;
        }
    }

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
            API = renderApi is IVulkanWindowRenderApi ? GraphicsAPI.DefaultVulkan : GraphicsAPI.Default
        });
        RenderApi = renderApi;
    }

    public void Initialize()
    {
        if (initialized) return;

        window.Initialize();

        InitInput();

        if (RenderApi is IVulkanWindowRenderApi)
        {
            if (window.VkSurface == null)
            {
                throw new Exception("Vulkan surface is null");
            }

            GlfwVulkanContextInfo info = new GlfwVulkanContextInfo(window.VkSurface!);
            RenderApi.CreateInstance(info, window.Size.ToVecI());
        }
        else if (RenderApi is IOpenGlWindowRenderApi)
        {
            RenderApi.CreateInstance(window.GLContext, window.Size.ToVecI());
        }
        else
        {
            RenderApi.CreateInstance(window.Native, window.Size.ToVecI());
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

        InputController = new InputController(keyboards, pointers);
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

        CreateRenderTarget(window!.FramebufferSize.ToVecI(), RenderApi.RenderTexture);
    }

    private void CreateRenderTarget(VecI size, ITexture nativeRenderTexture)
    {
        var drawingSurface =
            DrawingBackendApi.Current.CreateRenderSurface(size, nativeRenderTexture, SurfaceOrigin.TopLeft);
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

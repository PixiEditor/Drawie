using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.Vulkan.Buffers;
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
    public event Action<double> Render;

    private SKSurface? surface;

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

    public void Show()
    {
        if (!isRunning)
        {
            window.Initialize();
            RenderApi.CreateInstance(window.VkSurface, window.Size.ToVecI());
            window.FramebufferResize += WindowOnFramebufferResize;

            var vkRenderApi = (VulkanWindowRenderApi)RenderApi;
            var vkBackendContext = new GRVkBackendContext()
            {
                VkDevice = vkRenderApi.LogicalDevice.Handle,
                VkInstance = vkRenderApi.Instance.Handle,
                VkPhysicalDevice = vkRenderApi.PhysicalDevice.Handle,
                VkQueue = vkRenderApi.graphicsQueue.Handle,
                GraphicsQueueIndex = 0,
                GetProcedureAddress = vkRenderApi.GetProcedureAddress
            };

            var ctx = GRContext.CreateVulkan(vkBackendContext);

            var imageInfo = new GRVkImageInfo()
            {
                CurrentQueueFamily = 0,
                Format = vkRenderApi.texture.ImageFormat,
                Image = vkRenderApi.texture.VkImage.Handle,
                ImageLayout = VulkanTexture.ColorAttachmentOptimal,
                ImageTiling = vkRenderApi.texture.Tiling,
                ImageUsageFlags = vkRenderApi.texture.UsageFlags,
                LevelCount = 1,
                SampleCount = 1,
                Protected = false,
                SharingMode = vkRenderApi.texture.TargetSharingMode
            };

            surface = SKSurface.Create(ctx, new GRBackendRenderTarget(Size.X, Size.Y, 1, imageInfo),
                GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
            
            vkRenderApi.texture.TransitionLayoutTo(0, VulkanTexture.ShaderReadOnlyOptimal);

            window.Render += d =>
            {
                vkRenderApi.texture.TransitionLayoutTo(VulkanTexture.ShaderReadOnlyOptimal, VulkanTexture.ColorAttachmentOptimal);
            };
            window.Render += OnRender;
            window.Render += d =>
            {
                vkRenderApi.texture.TransitionLayoutTo(VulkanTexture.ColorAttachmentOptimal, VulkanTexture.ShaderReadOnlyOptimal);
            };
            window.Render += RenderApi.Render;
            
            window.Update += OnUpdate;
            isRunning = true;
            window.Run();
        }
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
        surface?.Canvas.Clear(SKColors.White);
        surface.Canvas.DrawText($"Hello Vulkan {dt}!", 500, 500, SKTextAlign.Center, new SKFont(SKTypeface.Default, 64),
            new SKPaint());
        surface!.Flush();
        Render?.Invoke(dt);
    }

    public void Close()
    {
        window.Update -= OnUpdate;
        window.Render -= OnRender;
        RenderApi.DestroyInstance();

        window?.Close();
        window?.Dispose();
    }
}
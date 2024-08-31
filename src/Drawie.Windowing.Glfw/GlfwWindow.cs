
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan;
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
            if (window != null)
            {
                window.Title = value;
            }
        }
    }
    public VecI Size
    {
        get => window?.Size.ToVecI() ?? VecI.Zero;
        set
        {
            if (window != null)
            {
                window.Size = value.ToVector2DInt();
            }
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
            
            /*VulkanWindowRenderApi vkRenderApi = (VulkanWindowRenderApi) RenderApi;
            GRVkBackendContext vkBackendContext = new GRVkBackendContext()
            {
                VkDevice = vkRenderApi.LogicalDevice.Handle,
                VkInstance = vkRenderApi.Instance.Handle,
                VkPhysicalDevice = vkRenderApi.PhysicalDevice.Handle,
                VkQueue = vkRenderApi.graphicsQueue.Handle,
                GraphicsQueueIndex = 0,
                GetProcedureAddress = vkRenderApi.GetProcedureAddress
            };
            
            GRContext ctx = GRContext.CreateVulkan(vkBackendContext);*/
            
            window.Render += RenderApi.Render;
            window.Render += OnRender;
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
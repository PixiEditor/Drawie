using System.Runtime.InteropServices;
using PixiEditor.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using Silk.NET.WebGPU.Extensions.WGPU;

namespace Drawie.RenderApi.WebGpu;

public class WebGpuWindowRenderApi : IWindowRenderApi
{
    public GraphicsApi GraphicsApi { get; }
    
    public event Action? FramebufferResized;

    private WebGPU wgpu;
    private Adapter adapter;

    private int deviceExtras = 0x00030001;
    
    public unsafe void CreateInstance(object surface, VecI framebufferSize)
    {
        if(surface is not INativeWindow nativeWindow)
        {
            throw new ArgumentException("Surface must be a window handle");
        }
        
        wgpu = WebGPU.GetApi();
        
        InstanceExtras instanceExtras = new InstanceExtras()
        {
            Chain = new ChainedStruct()
            {
                SType = (SType)deviceExtras
            },
            Backends = InstanceBackend.Vulkan
        };
        
        InstanceDescriptor instanceDescriptor = new InstanceDescriptor()
        {
            NextInChain = &instanceExtras.Chain
        };
        
        var instance = wgpu.CreateInstance(instanceDescriptor);

        var x11Surface = nativeWindow.X11;

        nuint surfaceHandle = x11Surface.Value.Window;
        IntPtr displayHandle = x11Surface.Value.Display;
        
        SurfaceDescriptorFromXlibWindow xlibSurfaceDescriptor = new SurfaceDescriptorFromXlibWindow()
        {
            Chain = new ChainedStruct()
            {
                SType = SType.SurfaceDescriptorFromXlibWindow
            },
            
            Window = surfaceHandle, 
            Display = &displayHandle
        };
        
        SurfaceDescriptor surfaceDescriptor = new SurfaceDescriptor()
        {
            NextInChain = &xlibSurfaceDescriptor.Chain
        };
        
        var wgpuSurface = wgpu.InstanceCreateSurface(instance, &surfaceDescriptor);

        var adapterOptions = new RequestAdapterOptions()
        {
            NextInChain = null,
            PowerPreference = PowerPreference.HighPerformance,
            CompatibleSurface = wgpuSurface,
            BackendType = BackendType.Vulkan
        };

        var requestAdapterCallback = new PfnRequestAdapterCallback(AdapterRequestEndedCallback);
        
        
        wgpu.InstanceRequestAdapter(instance, &adapterOptions, requestAdapterCallback, (void*)0);
    }

    private unsafe void AdapterRequestEndedCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userData)
    {
        if (status == RequestAdapterStatus.Success)
        {
            this.adapter = *adapter;

            AdapterProperties adapterProperties = default;
            
            wgpu.AdapterGetProperties(adapter, &adapterProperties);
            
            string name = Marshal.PtrToStringAnsi((IntPtr)adapterProperties.Name);
            
            Console.WriteLine($"Adapter name: {name}"); 
        }
    }

    public void DestroyInstance()
    {
        throw new NotImplementedException();
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        throw new NotImplementedException();
    }

    public void PrepareTextureToWrite()
    {
        throw new NotImplementedException();
    }

    public void Render(double deltaTime)
    {
        throw new NotImplementedException();
    }

}
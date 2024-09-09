namespace Drawie.RenderApi;

public interface IVulkanWindowRenderApi : IWindowRenderApi
{
    public IntPtr LogicalDeviceHandle { get; }
    public IntPtr PhysicalDeviceHandle { get;  }
    public IntPtr InstanceHandle { get;  }
    public IntPtr GraphicsQueueHandle { get;  }
    public IVkTexture RenderTexture { get; }
    public IntPtr GetProcedureAddress(string name, IntPtr instance, IntPtr device);
}
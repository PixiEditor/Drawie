using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.ContextObjects;

public class VulkanDevice : VulkanObject
{ 
    public Device Device { get; set; }
    
    public VulkanDevice(Vk vk, Device device) : base(vk)
    {
        Device = device;
    }

    public Result WaitIdle()
    {
        return Vk!.DeviceWaitIdle(Device);
    }
    
    public override unsafe void Dispose()
    {
        Vk.DestroyDevice(Device, null);
    }
}
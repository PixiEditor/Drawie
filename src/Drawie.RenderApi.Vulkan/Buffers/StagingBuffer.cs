using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class StagingBuffer : BufferObject
{
    public StagingBuffer(VulkanContext context, ulong size) : this(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice, size)
    {
    }
    
    public StagingBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size) : base(vk, device, physicalDevice, size, BufferUsageFlags.TransferSrcBit,
MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
    {
    }
}
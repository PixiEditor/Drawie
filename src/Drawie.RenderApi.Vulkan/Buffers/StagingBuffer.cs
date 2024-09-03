using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class StagingBuffer : BufferObject
{
    public StagingBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size) : base(vk, device, physicalDevice, size, BufferUsageFlags.TransferSrcBit,
MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
    {
    }
}
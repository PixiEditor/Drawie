using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class UniformBuffer : BufferObject
{
    public UniformBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size) : base(vk, device, physicalDevice, size, BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
    {
    }

}
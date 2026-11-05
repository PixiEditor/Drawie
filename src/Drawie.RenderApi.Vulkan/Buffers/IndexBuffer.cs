using Drawie.RenderApi.Abstraction.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class IndexBuffer : BufferObject
{
    public IndexBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size) 
        : base(vk, device, physicalDevice, size, BufferUsageFlags.TransferDstBit | BufferUsageFlags.IndexBufferBit, MemoryPropertyFlags.DeviceLocalBit, BufferUsage.Index)
    {
    }

    public IndexBuffer(VulkanContext context, ulong bufferSize) : this(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice, bufferSize)
    {
    } 
}
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Vulkan.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanStorageBuffer : BufferObject
{
    public VulkanStorageBuffer(VulkanContext context, ulong size)
        : base(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice, size, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, BufferUsage.Storage)
    {
    }
}
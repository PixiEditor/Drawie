using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class BufferObject : IDisposable
{
    public ulong Size { get; set; }
    
    public Buffer VkBuffer => vkBuffer;
    public DeviceMemory VkBufferMemory => vkBufferMemory;

    private Silk.NET.Vulkan.Buffer vkBuffer = default;
    private DeviceMemory vkBufferMemory = default;

    private Vk vk;
    private Device device;
    private PhysicalDevice physicalDevice;

    protected unsafe BufferObject(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size,
        BufferUsageFlags usage,
        MemoryPropertyFlags properties)
    {
        Size = size;
        this.vk = vk;
        this.device = device;
        this.physicalDevice = physicalDevice;

        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        fixed (Buffer* bufferPtr = &vkBuffer)
        {
            if (vk!.CreateBuffer(device, bufferInfo, null, bufferPtr) != Result.Success)
                throw new VulkanException("Failed to create vertex buffer.");
        }

        vk!.GetBufferMemoryRequirements(device, vkBuffer, out var memoryRequirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memoryRequirements.Size,
            MemoryTypeIndex = FindMemoryType(vk, this.physicalDevice, memoryRequirements.MemoryTypeBits, properties)
        };

        fixed (DeviceMemory* bufferMemoryPtr = &vkBufferMemory)
        {
            if (vk!.AllocateMemory(device, in allocInfo, null, bufferMemoryPtr) != Result.Success)
                throw new VulkanException("Failed to allocate vertex buffer memory.");
        }

        vk!.BindBufferMemory(device, vkBuffer, vkBufferMemory, 0);
    }

    public unsafe void SetData<T>(T[] data) where T : unmanaged
    {
        void* dataPtr;
        vk!.MapMemory(device, vkBufferMemory, 0, Size, 0, &dataPtr);
        data.AsSpan().CopyTo(new Span<T>(dataPtr, data.Length));
        vk!.UnmapMemory(device, vkBufferMemory);
    }

    public static uint FindMemoryType(Vk vk, PhysicalDevice device, uint typeFilter, MemoryPropertyFlags properties)
    {
        vk!.GetPhysicalDeviceMemoryProperties(device, out var memProperties);

        for (int i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << i)) != 0 && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
            {
                return (uint)i;
            }
        }

        throw new VulkanException("Failed to find suitable memory type.");
    }

    public unsafe void Dispose()
    {
        vk!.DestroyBuffer(device, vkBuffer, null);
        vk!.FreeMemory(device, vkBufferMemory, null);
    }
}
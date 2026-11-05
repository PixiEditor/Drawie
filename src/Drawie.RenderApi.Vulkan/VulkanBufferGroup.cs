using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Vulkan.Buffers;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanBufferGroup : IBufferGroup
{
    private static uint counter;

    public uint Handle { get; } = counter++;

    private VulkanBufferGroupList buffers = new VulkanBufferGroupList();

    public void Open(Action<IBufferGroupList> list)
    {
        list(buffers);
    }
}
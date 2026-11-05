using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Vulkan.Buffers;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanBufferGroupList : IBufferGroupList
{
    public List<IBuffer> Buffers { get; } = new();

    public IVkBuffer? IndexBuffer => Buffers.FirstOrDefault(x => x.Usage == BufferUsage.Index) as IVkBuffer;
    public IVkBuffer? VertexBuffer => Buffers.FirstOrDefault(x => x.Usage == BufferUsage.Vertex)  as IVkBuffer;

    public VulkanBufferGroupList()
    {
    }
}
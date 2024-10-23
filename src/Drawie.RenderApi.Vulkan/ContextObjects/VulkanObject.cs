using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.ContextObjects;

public abstract class VulkanObject : IDisposable
{
    public Vk Vk { get; }
    protected VulkanObject(Vk vk)
    {
        Vk = vk;
    }

    public abstract void Dispose();
}
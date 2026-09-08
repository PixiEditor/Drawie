using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanDescriptorSetLayout : IDisposable
{
    public VulkanContext Context { get; }
    public DescriptorSetLayout DescriptorSetLayout { get; }
    public string[] Bindings { get; }

    public VulkanDescriptorSetLayout(VulkanContext context, DescriptorSetLayout descriptorSetLayout, string[] bindings)
    {
        Context = context;
        DescriptorSetLayout = descriptorSetLayout;
        Bindings = bindings;
    }

    public unsafe void Dispose()
    {
        Context.Api.DestroyDescriptorSetLayout(Context.LogicalDevice.Device, DescriptorSetLayout, null);
    }
}
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanDescriptorSetLayout
{
    public DescriptorSetLayout DescriptorSetLayout { get; }
    public string[] Bindings { get; }

    public VulkanDescriptorSetLayout(DescriptorSetLayout descriptorSetLayout, string[] bindings)
    {
        DescriptorSetLayout = descriptorSetLayout;
        Bindings = bindings;
    }
}
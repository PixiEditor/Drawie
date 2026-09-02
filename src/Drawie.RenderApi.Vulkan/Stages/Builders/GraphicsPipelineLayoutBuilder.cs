namespace Drawie.RenderApi.Vulkan.Stages.Builders;

using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

public class GraphicsPipelineLayoutBuilder
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }

    private readonly List<DescriptorSetLayout> descriptorSetLayouts = new();

    private PipelineLayout? layout;

    public GraphicsPipelineLayoutBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public GraphicsPipelineLayoutBuilder AddDescriptorSetLayout(DescriptorSetLayout layout)
    {
        descriptorSetLayouts.Add(layout);
        return this;
    }

    public GraphicsPipelineLayoutBuilder WithDescriptorSetLayouts(
        params DescriptorSetLayout[] layouts)
    {
        descriptorSetLayouts.Clear();
        descriptorSetLayouts.AddRange(layouts);
        return this;
    }

    public unsafe PipelineLayout Create()
    {
        if (layout != null) return layout.Value;
            
        DescriptorSetLayout* layouts = stackalloc DescriptorSetLayout[descriptorSetLayouts.Count];

        for (int i = 0; i < descriptorSetLayouts.Count; i++)
        {
            layouts[i] = descriptorSetLayouts[i];
        }

        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            PushConstantRangeCount = 0,
            SetLayoutCount = (uint)descriptorSetLayouts.Count,
            PSetLayouts = descriptorSetLayouts.Count > 0 ? layouts : null
        };

        if (Vk.CreatePipelineLayout(
                LogicalDevice,
                in pipelineLayoutInfo,
                null,
                out var pipelineLayout) != Result.Success)
        {
            throw new VulkanException("Failed to create pipeline layout.");
        }

        layout = pipelineLayout;

        return pipelineLayout;
    }
}
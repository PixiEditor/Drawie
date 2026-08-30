using Drawie.Backend.Shaders.Common;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class GraphicsPipelineDescriptorBuilder
{
    public List<GraphicsPipelineDescriptorBindingBuilder> BindingBuilders { get; private set; } =
        new List<GraphicsPipelineDescriptorBindingBuilder>();

    public GraphicsPipelineDescriptorBuilder WithBinding(
        Action<GraphicsPipelineDescriptorBindingBuilder> bindingBuilder)
    {
        var builder = new GraphicsPipelineDescriptorBindingBuilder();
        bindingBuilder(builder);
        BindingBuilders.Add(builder);
        return this;
    }

    public DescriptorSetLayoutBinding[] Build()
    {
        if (BindingBuilders.Count == 0) 
            return Array.Empty<DescriptorSetLayoutBinding>();

        var flattened = FlattenBuilders(BindingBuilders);

        return flattened.Select(b => b.Build()).ToArray();
    }

    private List<GraphicsPipelineDescriptorBindingBuilder> FlattenBuilders(List<GraphicsPipelineDescriptorBindingBuilder> bindingBuilders)
    {
        var flattened = new List<GraphicsPipelineDescriptorBindingBuilder>();
        
        foreach (var bindingBuilder in bindingBuilders)
        {
            if(flattened.Any(x => x.Binding == bindingBuilder.Binding)) continue;
            flattened.Add(bindingBuilder);
        }

        return flattened;
    }
}

public class GraphicsPipelineDescriptorBindingBuilder
{
    public int Binding { get; set; }
    public DescriptorType DescriptorType { get; set; }
    public ShaderStageFlags ShaderStageFlags { get; set; }

    public GraphicsPipelineDescriptorBindingBuilder AtPosition(int position)
    {
        Binding = position;
        return this;
    }

    public GraphicsPipelineDescriptorBindingBuilder ForStages(ShaderStageFlags stageFlags)
    {
        ShaderStageFlags = stageFlags;
        return this;
    }

    public GraphicsPipelineDescriptorBindingBuilder OfType(DescriptorType type)
    {
        DescriptorType = type;
        return this;
    }

    public unsafe DescriptorSetLayoutBinding Build()
    {
        if (Binding < 0) throw new ArgumentException("Binding can't be negative");
        
        return new DescriptorSetLayoutBinding((uint)Binding, DescriptorType, 1, ShaderStageFlags);
    }
}
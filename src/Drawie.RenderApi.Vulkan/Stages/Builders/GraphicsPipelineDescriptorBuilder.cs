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

    public Dictionary<int, List<DescriptorSetLayoutBinding>> Build()
    {
        if (BindingBuilders.Count == 0)
            return new Dictionary<int, List<DescriptorSetLayoutBinding>>();

        var dict = new Dictionary<int, List<DescriptorSetLayoutBinding>>();
        for (int i = 0; i < BindingBuilders.Count; i++)
        {
            var builder = BindingBuilders[i];
            if (!dict.ContainsKey(builder.Set))
            {
                dict[builder.Set] = new List<DescriptorSetLayoutBinding>();
            }
            
            dict[builder.Set].Add(builder.Build());
        }
        
        return dict;
    }
}

public class GraphicsPipelineDescriptorBindingBuilder
{
    public int Set { get; set; }
    public int Binding { get; set; }
    public DescriptorType DescriptorType { get; set; }
    public ShaderStageFlags ShaderStageFlags { get; set; }
    public string Name { get; set; }

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
    
    public GraphicsPipelineDescriptorBindingBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public unsafe DescriptorSetLayoutBinding Build()
    {
        if (Binding < 0) throw new ArgumentException("Binding can't be negative");
        
        return new DescriptorSetLayoutBinding((uint)Binding, DescriptorType, 1, ShaderStageFlags);
    }

    public GraphicsPipelineDescriptorBindingBuilder AtSet(int set)
    {
        Set = set;
        return this;
    }
}
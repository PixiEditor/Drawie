using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class GraphicsPipelineVertexLayoutBuilder
{
    public List<VertexAttributeLayout> Components { get; set; } = new();
    public uint Binding { get; set; } = 0;


    public GraphicsPipelineVertexLayoutBuilder WithVec2()
    {
        Components.Add(new VertexAttributeLayout { ComponentCount = 2, Format = Format.R32G32Sfloat });
        return this;
    }

    public GraphicsPipelineVertexLayoutBuilder WithVec3()
    {
        Components.Add(new VertexAttributeLayout() { ComponentCount = 3, Format = Format.R32G32B32Sfloat });
        return this;
    }

    public unsafe (VertexInputBindingDescription, VertexInputAttributeDescription[]) Build()
    {
        int stride = 0;
        foreach (var component in Components)
        {
            stride += component.Size;
        }

        var bindingDesc = new VertexInputBindingDescription
        {
            Binding = Binding,
            Stride = (uint)stride,
            InputRate = VertexInputRate.Vertex
        };

        var descriptions = new VertexInputAttributeDescription[Components.Count];
        int offset = 0;
        for (var index = 0; index < Components.Count; index++)
        {
            var component = Components[index];
            descriptions[index] = new VertexInputAttributeDescription()
            {
                Binding = Binding,
                Location = (uint)index,
                Format = component.Format,
                Offset = (uint)offset,
            };

            offset += component.ComponentCount * sizeof(float);
        }

        return (bindingDesc, descriptions);
    }
}

public struct VertexAttributeLayout
{
    public Format Format { get; set; }
    public int ComponentCount { get; set; }

    public int Size => ComponentCount * sizeof(float);
}
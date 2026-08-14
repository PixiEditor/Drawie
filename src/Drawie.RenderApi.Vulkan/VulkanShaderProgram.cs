using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Silk.NET.Vulkan;
using Buffer = System.Buffer;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanShaderProgram : IShaderProgram
{
    public VulkanContext Context { get; }
    public ShaderProgramDesc Description { get; }
    public List<UniformBlock> UniformBlocks { get; private set; } = new();

    public GraphicsPipelineStageBuilder VertexStageBuilder { get; }
    public GraphicsPipelineStageBuilder FragmentStageBuilder { get; }
    public DescriptorSetLayout DescriptorSetLayout { get; set; }

    public VulkanShaderProgram(VulkanContext context, ShaderProgramDesc desc)
    {
        Context = context;
        Description = desc;

        foreach (var shader in desc.Shaders)
        {
            GraphicsPipelineStageBuilder stageBuilder =
                new GraphicsPipelineStageBuilder(Context.Api, Context.LogicalDevice.Device);

            stageBuilder.ShaderBytes = shader.Bytes;
            stageBuilder.EntryName = shader.EntryName;
            if (shader.Type == ShaderType.Vertex)
            {
                VertexStageBuilder = stageBuilder;
                stageBuilder.Type = GraphicsPipelineStageType.Vertex;
            }
            else if (shader.Type == ShaderType.Fragment)
            {
                FragmentStageBuilder = stageBuilder;
                stageBuilder.Type = GraphicsPipelineStageType.Fragment;
            }
            else
            {
                throw new NotImplementedException("Unsupported shader type.");
            }
        }

        DescriptorSetLayout = CreateDescriptorSetLayout();
    }

    public void Use()
    {
    }

    public void UpdateUniforms(List<UniformBlock> blocks)
    {
        UniformBlocks = new List<UniformBlock>(blocks);
        foreach (var uniformBlock in UniformBlocks)
        {
            Serialize(uniformBlock);
        }
    }

    private void Serialize(UniformBlock block)
    {
        var data = new byte[block.ShaderLayout.Size];

        foreach (var property in block.Properties)
        {
            var layout = block.ShaderLayout.UniformProperties
                .FirstOrDefault(x => x.Name == property.UniformName);

            Write(
                data,
                layout.Offset,
                property.ObjValue);
        }
        
        // TODO: Write to shader
    }

    private void Write(
        byte[] destination,
        int offset,
        object value)
    {
        switch (value)
        {
            case float v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(float)),
                    v);
                break;

            case int v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(int)),
                    v);
                break;

            case uint v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(uint)),
                    v);
                break;

            case Vector2 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;

            case Vector3 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;

            case Vector4 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;

            case Matrix4x4 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;

            default:
                throw new NotSupportedException(
                    $"Cannot serialize uniform value of type {value.GetType()}.");
        }
    }
    
    private unsafe DescriptorSetLayout CreateDescriptorSetLayout()
    {
        DescriptorSetLayoutBinding[] bindings =
        [
            new()
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.VertexBit
            },

            new()
            {
                Binding = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.FragmentBit
            }
        ];

        fixed (DescriptorSetLayoutBinding* bindingPtr = bindings)
        {
            DescriptorSetLayoutCreateInfo info = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindings = bindingPtr
            };

            if (Context.Api!.CreateDescriptorSetLayout(
                    Context.LogicalDevice.Device,
                    in info,
                    null,
                    out var layout) != Result.Success)
            {
                throw new VulkanException(
                    "Failed to create Vulkan descriptor set layout.");
            }

            return layout;
        }
    }
}
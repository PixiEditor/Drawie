using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Extensions;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Silk.NET.Vulkan;
using Buffer = System.Buffer;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanShaderProgram : IShaderProgram, IDisposable
{
    public VulkanContext Context { get; }
    public ShaderProgramDesc Description { get; }

    public GraphicsPipelineStageBuilder VertexStageBuilder { get; }
    public GraphicsPipelineStageBuilder FragmentStageBuilder { get; }
    public DescriptorSetLayout DescriptorSetLayout { get; set; }
    
    public VulkanDescriptorPool DescriptorPool { get; set; }


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
        DescriptorPool = CreateDescriptorPool();
    }

    public void Use()
    {
    }

    private unsafe VulkanDescriptorPool CreateDescriptorPool()
    {
        DescriptorPoolSize poolSize = new DescriptorPoolSize()
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
        };
        
        DescriptorPoolSize poolFragSize = new DescriptorPoolSize()
        {
            Type = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
        };

        DescriptorPoolSize* poolSizes = stackalloc DescriptorPoolSize[2];
        poolSizes[0] = poolSize;
        poolSizes[1] = poolFragSize;

        DescriptorPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = 2,
            PPoolSizes = poolSizes,
            MaxSets = 10
        };
        
        Context.Api.CreateDescriptorPool(Context.LogicalDevice.Device, &poolInfo, null, out var descriptorPool).ThrowOnError("Failed to create descriptor pool.");
        
        return new VulkanDescriptorPool(Context, descriptorPool, [DescriptorSetLayout]);
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

    public unsafe void Dispose()
    {
        Context.Api.DestroyDescriptorSetLayout(Context.LogicalDevice.Device, DescriptorSetLayout, null);
        DescriptorPool.Dispose();
    }
}
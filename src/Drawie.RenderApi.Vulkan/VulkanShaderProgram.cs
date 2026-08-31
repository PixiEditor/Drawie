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
    public GraphicsPipelineVertexLayoutBuilder VertexLayoutBuilder { get; }
    public GraphicsPipelineStageBuilder FragmentStageBuilder { get; }
    public VulkanDescriptorSetLayout DescriptorSetLayout { get; set; }
    public VulkanDescriptorPool DescriptorPool { get; set; }


    public VulkanShaderProgram(VulkanContext context, ShaderProgramDesc desc)
    {
        Context = context;
        Description = desc;

        GraphicsPipelineDescriptorBuilder descriptorBuilder = new GraphicsPipelineDescriptorBuilder();

        foreach (var shader in desc.Shaders)
        {
            GraphicsPipelineStageBuilder stageBuilder =
                new GraphicsPipelineStageBuilder(Context.Api, Context.LogicalDevice.Device);

            stageBuilder.ShaderBytes = shader.ShaderBytes;
            stageBuilder.EntryName = shader.EntryName;
            if (shader.ShaderType == ShaderType.Vertex)
            {
                VertexStageBuilder = stageBuilder;
                stageBuilder.Type = GraphicsPipelineStageType.Vertex;
                if (VertexLayoutBuilder == null)
                {
                    VertexLayoutBuilder = new GraphicsPipelineVertexLayoutBuilder();
                    BuildVertexLayoutFromReflection(
                        shader.Reflection.EntryPoints.FirstOrDefault(x => x.Type == ShaderType.Vertex));
                    if (VertexLayoutBuilder.Components.Count == 0)
                    {
                        VertexLayoutBuilder = null;
                    }
                }
            }
            else if (shader.ShaderType == ShaderType.Fragment)
            {
                FragmentStageBuilder = stageBuilder;
                stageBuilder.Type = GraphicsPipelineStageType.Fragment;
            }
            else
            {
                throw new NotImplementedException("Unsupported shader type.");
            }

            foreach (var reflectionParameter in shader.Reflection.Parameters)
            {
                descriptorBuilder.WithBinding(bindingBuilder =>
                {
                    bindingBuilder.AtPosition(reflectionParameter.Index)
                        .ForStages(shader.ShaderType == ShaderType.Vertex
                            ? ShaderStageFlags.VertexBit
                            : ShaderStageFlags.FragmentBit)
                        .OfType(ToDescriptorType(reflectionParameter.Var.Type, reflectionParameter.Var.ResourceType))
                        .WithName(reflectionParameter.Name);
                });
            }
        }

        var descriptors = descriptorBuilder.Build();
        DescriptorSetLayout = CreateDescriptorSetLayout(descriptors, descriptorBuilder);
        DescriptorPool = CreateDescriptorPool(descriptors);
    }

    public void Use()
    {
    }
    
    private void BuildVertexLayoutFromReflection(EntryPoint? vertexEntryPoint)
    {
        if (vertexEntryPoint == null) return;
        
        foreach (var param in vertexEntryPoint.Params)
        {
            if(!param.HasBindings) continue;
            foreach (var field in param.Fields)
            {
                VertexLayoutBuilder.Components.Add(new VertexAttributeLayout(ScalarToFormat(field.ScalarType, field.ScalarsCount),
                    field.Size));
            }
        }
    }

   private Format ScalarToFormat(ScalarType? fieldScalarType, int fieldScalarsCount)
{
    if (fieldScalarType == null) return Format.Undefined;

    return (fieldScalarType.Value, fieldScalarsCount) switch
    {
        (ScalarType.Unknown, _) => Format.Undefined,
        (ScalarType.Void, _) => Format.Undefined,

        (ScalarType.Bool, 1) => Format.R8Uint,
        (ScalarType.Bool, 2) => Format.R8G8Uint,
        (ScalarType.Bool, 3) => Format.R8G8B8Uint,
        (ScalarType.Bool, 4) => Format.R8G8B8A8Uint,

        (ScalarType.Int8, 1) => Format.R8Sint,
        (ScalarType.Int8, 2) => Format.R8G8Sint,
        (ScalarType.Int8, 3) => Format.R8G8B8Sint,
        (ScalarType.Int8, 4) => Format.R8G8B8A8Sint,

        (ScalarType.UInt8, 1) => Format.R8Uint,
        (ScalarType.UInt8, 2) => Format.R8G8Uint,
        (ScalarType.UInt8, 3) => Format.R8G8B8Uint,
        (ScalarType.UInt8, 4) => Format.R8G8B8A8Uint,

        (ScalarType.Int16, 1) => Format.R16Sint,
        (ScalarType.Int16, 2) => Format.R16G16Sint,
        (ScalarType.Int16, 3) => Format.R16G16B16Sint,
        (ScalarType.Int16, 4) => Format.R16G16B16A16Sint,

        (ScalarType.UInt16, 1) => Format.R16Uint,
        (ScalarType.UInt16, 2) => Format.R16G16Uint,
        (ScalarType.UInt16, 3) => Format.R16G16B16Uint,
        (ScalarType.UInt16, 4) => Format.R16G16B16A16Uint,

        (ScalarType.Int32, 1) => Format.R32Sint,
        (ScalarType.Int32, 2) => Format.R32G32Sint,
        (ScalarType.Int32, 3) => Format.R32G32B32Sint,
        (ScalarType.Int32, 4) => Format.R32G32B32A32Sint,

        (ScalarType.UInt32, 1) => Format.R32Uint,
        (ScalarType.UInt32, 2) => Format.R32G32Uint,
        (ScalarType.UInt32, 3) => Format.R32G32B32Uint,
        (ScalarType.UInt32, 4) => Format.R32G32B32A32Uint,

        (ScalarType.Int64, 1) => Format.R64Sint,
        (ScalarType.Int64, 2) => Format.R64G64Sint,
        (ScalarType.Int64, 3) => Format.R64G64B64Sint,
        (ScalarType.Int64, 4) => Format.R64G64B64A64Sint,

        (ScalarType.UInt64, 1) => Format.R64Uint,
        (ScalarType.UInt64, 2) => Format.R64G64Uint,
        (ScalarType.UInt64, 3) => Format.R64G64B64Uint,
        (ScalarType.UInt64, 4) => Format.R64G64B64A64Uint,

        (ScalarType.Float16, 1) => Format.R16Sfloat,
        (ScalarType.Float16, 2) => Format.R16G16Sfloat,
        (ScalarType.Float16, 3) => Format.R16G16B16Sfloat,
        (ScalarType.Float16, 4) => Format.R16G16B16A16Sfloat,

        (ScalarType.Float32, 1) => Format.R32Sfloat,
        (ScalarType.Float32, 2) => Format.R32G32Sfloat,
        (ScalarType.Float32, 3) => Format.R32G32B32Sfloat,
        (ScalarType.Float32, 4) => Format.R32G32B32A32Sfloat,

        (ScalarType.Float64, 1) => Format.R64Sfloat,
        (ScalarType.Float64, 2) => Format.R64G64Sfloat,
        (ScalarType.Float64, 3) => Format.R64G64B64Sfloat,
        (ScalarType.Float64, 4) => Format.R64G64B64A64Sfloat,

        _ => throw new ArgumentOutOfRangeException(nameof(fieldScalarsCount), fieldScalarsCount, null)
    };
}

    private unsafe VulkanDescriptorPool CreateDescriptorPool(DescriptorSetLayoutBinding[] descriptors)
    {
        Dictionary<DescriptorType, int> pools = new Dictionary<DescriptorType, int>();
        foreach (var descriptorSetLayoutBinding in descriptors)
        {
            pools.TryAdd(descriptorSetLayoutBinding.DescriptorType, 0);
            pools[descriptorSetLayoutBinding.DescriptorType]++;
        }

        DescriptorPoolSize* poolSizes = stackalloc DescriptorPoolSize[pools.Count];

        int index = 0;
        foreach (var pool in pools)
        {
            poolSizes[index] = new DescriptorPoolSize(pool.Key, (uint)pool.Value);
            index++;
        }

        DescriptorPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = (uint)pools.Count,
            PPoolSizes = poolSizes,
            MaxSets = 20 // no reasoning behind it, change if necessary
        };

        Context.Api.CreateDescriptorPool(Context.LogicalDevice.Device, &poolInfo, null, out var descriptorPool)
            .ThrowOnError("Failed to create descriptor pool.");

        return new VulkanDescriptorPool(Context, descriptorPool, [DescriptorSetLayout.DescriptorSetLayout]);
    }

    private unsafe VulkanDescriptorSetLayout CreateDescriptorSetLayout(DescriptorSetLayoutBinding[] descriptors,
        GraphicsPipelineDescriptorBuilder descriptorBuilder)
    {
        fixed (DescriptorSetLayoutBinding* bindingPtr = descriptors)
        {
            DescriptorSetLayoutCreateInfo info = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)descriptors.Length,
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

            return new VulkanDescriptorSetLayout(layout,
                descriptorBuilder.BindingBuilders.OrderBy(x => x.Binding).Select(y => y.Name).ToArray());
        }
    }

    private DescriptorType ToDescriptorType(ShaderVarType varType, ShaderVarShape? varResourceType)
    {
        switch (varType)
        {
            case ShaderVarType.None:
                break;
            case ShaderVarType.Struct:
                break;
            case ShaderVarType.Array:
                break;
            case ShaderVarType.Matrix:
                break;
            case ShaderVarType.Vector:
                break;
            case ShaderVarType.Scalar:
                break;
            case ShaderVarType.ConstantBuffer:
                return DescriptorType.UniformBuffer;
            case ShaderVarType.Resource:
                return ResourceTypeToDescriptor(varResourceType);
            case ShaderVarType.SamplerState:
                break;
            case ShaderVarType.TextureBuffer:
                break;
            case ShaderVarType.ShaderStorageBuffer:
                return DescriptorType.StorageBuffer;
            case ShaderVarType.ParameterBlock:
                break;
            case ShaderVarType.GenericTypeParameter:
                break;
            case ShaderVarType.Interface:
                break;
            case ShaderVarType.Feedback:
                break;
            case ShaderVarType.Pointer:
                break;
            case ShaderVarType.DynamicResource:
                break;
            case ShaderVarType.OutputStream:
                break;
            case ShaderVarType.MeshOutput:
                break;
            case ShaderVarType.Specialized:
                break;
        }

        throw new ArgumentOutOfRangeException(nameof(varType), varType, null);
    }

    private DescriptorType ResourceTypeToDescriptor(ShaderVarShape? varResourceType)
    {
        switch (varResourceType)
        {
            case ShaderVarShape.Texture2D:
                return DescriptorType.CombinedImageSampler;
            case ShaderVarShape.StructuredBuffer: 
                return DescriptorType.StorageBuffer;
            /*
            case ShaderVarShape.Unknown:
                break;
            case ShaderVarShape.Texture1D:
                break;
            case ShaderVarShape.Texture3D:
                break;
            case ShaderVarShape.TextureCube:
                break;
            case ShaderVarShape.TextureBuffer:
                break;
            case ShaderVarShape.ByteAddressBuffer:
                break;
            case ShaderVarShape.AccelerationStructure:
                break;
            case null:
                break;
            */
            default:
                throw new ArgumentOutOfRangeException(nameof(varResourceType), varResourceType, null);
        }
    }

    public unsafe void Dispose()
    {
        Context.Api.DestroyDescriptorSetLayout(Context.LogicalDevice.Device, DescriptorSetLayout.DescriptorSetLayout, null);
        DescriptorPool.Dispose();
    }
}
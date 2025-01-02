using System.Reflection;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class GraphicsPipelineStageBuilder : IDisposable
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }

    public GraphicsPipelineStageType Type { get; set; }
    public string ShaderPath { get; set; }

    public string EntryName { get; set; } = "main";

    private PipelineShaderStageCreateInfo createdStage;
    private ShaderModule shaderModule;
    private bool created;

    public GraphicsPipelineStageBuilder()
    {
    }

    public GraphicsPipelineStageBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public unsafe PipelineShaderStageCreateInfo Build()
    {
        if (created)
        {
            throw new GraphicsPipelineBuilderException("Stage was already created");
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ShaderPath);
        if (stream == null)
        {
            throw new GraphicsPipelineBuilderException("Shader file not found");
        }

        byte[] code = new byte[stream.Length];
        var read = stream.Read(code, 0, code.Length);
        if (read != code.Length)
        {
            throw new GraphicsPipelineBuilderException("Failed to read shader file");
        }

        shaderModule = CreateShaderModule(code);

        PipelineShaderStageCreateInfo stageCreateInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = Type switch
            {
                GraphicsPipelineStageType.Vertex => ShaderStageFlags.VertexBit,
                GraphicsPipelineStageType.Fragment => ShaderStageFlags.FragmentBit,
                _ => throw new GraphicsPipelineBuilderException("Invalid stage type")
            },
            Module = shaderModule,
            PName = (byte*)Marshal.StringToHGlobalAnsi(EntryName)
        };

        createdStage = stageCreateInfo;
        created = true;

        return stageCreateInfo;
    }

    public GraphicsPipelineStageBuilder OfType(GraphicsPipelineStageType type)
    {
        Type = type;
        return this;
    }

    public GraphicsPipelineStageBuilder WithShader(string path)
    {
        ShaderPath = path;
        return this;
    }

    public GraphicsPipelineStageBuilder WithEntryName(string name)
    {
        EntryName = name;
        return this;
    }

    private unsafe ShaderModule CreateShaderModule(byte[] code)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo, CodeSize = (nuint)code.Length
        };

        ShaderModule module;

        fixed (byte* codePtr = code)
        {
            createInfo.PCode = (uint*)codePtr;
            if (Vk!.CreateShaderModule(LogicalDevice, in createInfo, null, out module) != Result.Success)
                throw new VulkanException("Failed to create shader module");
        }

        return module;
    }

    public unsafe void Dispose()
    {
        Vk.DestroyShaderModule(LogicalDevice, shaderModule, null);
        SilkMarshal.Free((nint)createdStage.PName);
    }
}

public enum GraphicsPipelineStageType
{
    Vertex,
    Fragment
}

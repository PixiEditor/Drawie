using Drawie.Backend.Vertie.Core;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Helpers;
using Drawie.RenderApi.Vulkan.Stages;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanPipeline : IPipeline, IDisposable
{
    private readonly VulkanContext context;

    public PipelineDesc Description { get; }

    public Pipeline Pipeline { get; }
    public GraphicsPipeline GraphicsPipeline => graphicsPipeline;
    public VulkanDescriptorPool DescriptorPool => program.DescriptorPool;
    public VulkanShaderProgram Program => program;

    public GraphicsPipelineBuilder Builder { get; }

    private VulkanShaderProgram program;

    private GraphicsPipeline graphicsPipeline;

    public VulkanPipeline(
        VulkanContext context,
        PipelineDesc desc)
    {
        this.context = context;
        Description = desc;
        if (desc.ShaderProgram is not VulkanShaderProgram vulkanShaderProgram)
            throw new ArgumentException("Invalid Shader Program type");
        program = vulkanShaderProgram;

        Builder = new GraphicsPipelineBuilder(context.Api, context.LogicalDevice.Device);
        graphicsPipeline = CreatePipeline();
        Pipeline = graphicsPipeline.VkPipeline;
    }

    public unsafe void Apply(ICommandList cmdList)
    {
        if (cmdList is not VulkanCommandList commandList)
            throw new ArgumentNullException("Only vulkan command list is supported");

        context.Api!.CmdBindPipeline(
            commandList.CommandBuffer,
            PipelineBindPoint.Graphics,
            Pipeline);

        /*
        context.Api.CmdBindDescriptorSets(commandList.CommandBuffer, PipelineBindPoint.Graphics,
            GraphicsPipeline.VkPipelineLayout, 0, 1, in descriptorSet, 0, null);
    */
    }

    private GraphicsPipeline CreatePipeline()
    {
        Builder.VertexLayoutBuilder = program.VertexLayoutBuilder;
        Builder.WithPolygonMode(Description.Rasterizer.RenderMode == RenderMode.Default
                ? PolygonMode.Fill
                : PolygonMode.Line)
            .WithCullMode(ToCullFlags(Description.Rasterizer.CullMode))
            .WithFrontFace(FrontFace.Clockwise);

        Builder.Stages.Add(program.VertexStageBuilder);
        Builder.Stages.Add(program.FragmentStageBuilder);
        Builder.DoNotDisposeStages = true;
        Builder.WithRenderPass(builder =>
        {
            if (Description.Depth.Enabled)
            {
                builder.WithDepth(Description.Depth.Format.ToVkFormat())
                    .WithSamples(Description.Rasterizer.Samples);
            }
        });
        Builder.WithDepth();
        
        var pipeline = Builder.Create(new Extent2D((uint)Description.Viewport.Width, (uint)Description.Viewport.Height),
            Format.R8G8B8A8Unorm, ImageLayout.ColorAttachmentOptimal, [program.DescriptorSetLayout.DescriptorSetLayout]);

        return pipeline;
    }

    private CullModeFlags ToCullFlags(CullMode rasterizerCullMode)
    {
        return rasterizerCullMode switch
        {
            CullMode.None => CullModeFlags.None,
            CullMode.Back => CullModeFlags.BackBit,
            CullMode.Front => CullModeFlags.FrontBit,
            CullMode.BackAndFront => CullModeFlags.FrontAndBack,
            _ => throw new ArgumentOutOfRangeException(nameof(rasterizerCullMode), rasterizerCullMode, null)
        };
    }

    public void Dispose()
    {
        graphicsPipeline.Dispose();
    }
}
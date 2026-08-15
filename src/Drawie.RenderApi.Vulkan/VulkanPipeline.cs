using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Extensions;
using Drawie.RenderApi.Vulkan.Stages;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanPipeline : IPipeline, IDisposable
{
    private readonly VulkanContext context;

    public PipelineDesc Description { get; }

    public Pipeline Pipeline { get; }
    public GraphicsPipeline GraphicsPipeline => graphicsPipeline;
    public DescriptorSet DescriptorSet => program.DescriptorSet;

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
        if(cmdList is not VulkanCommandList commandList) throw new ArgumentNullException("Only vulkan command list is supported");
        
        context.Api!.CmdBindPipeline(
            commandList.CommandBuffer,
            PipelineBindPoint.Graphics,
            Pipeline);
        
        var descriptorSet = program.DescriptorSet;
        context.Api.CmdBindDescriptorSets(commandList.CommandBuffer, PipelineBindPoint.Graphics, GraphicsPipeline.VkPipelineLayout, 0, 1, &descriptorSet, 0, null);
    }

    private GraphicsPipeline CreatePipeline()
    {
        Builder.WithVertexLayout(layout => layout.WithVec3().WithVec3().WithVec2());
        Builder.Stages.Add(program.VertexStageBuilder);
        Builder.Stages.Add(program.FragmentStageBuilder);
        Builder.WithRenderPass(builder => {});
        Builder.WithDepth();
        var descriptorSetLayout = program.DescriptorSetLayout;
        var pipeline = Builder.Create(new Extent2D((uint)Description.Viewport.Width, (uint)Description.Viewport.Height),
            Format.R8G8B8A8Unorm, ImageLayout.PresentSrcKhr, ref descriptorSetLayout);

        return pipeline;
    }

    public void Dispose()
    {
        graphicsPipeline.Dispose();
    }
}
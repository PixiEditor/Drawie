using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanRecordedRenderPass : RecordedRenderPass
{
    public CommandBuffer CommandBuffer { get; }
    public VulkanContext Context { get; }
    public CommandPool Pool { get; }

    private bool submitted;

    public VulkanRecordedRenderPass(
        VulkanContext context,
        CommandBuffer commandBuffer,
        CommandPool commandPool)
    {
        CommandBuffer = commandBuffer;
        Context = context;
        Pool = commandPool;

        Execute = Submit;
    }

    private unsafe void Submit()
    {
        if (submitted)
            throw new InvalidOperationException(
                "This Vulkan render pass has already been submitted.");

        submitted = true;

        var commandBuffer = CommandBuffer;

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        var result = Context.Api!.QueueSubmit(
            Context.GraphicsQueue,
            1,
            in submitInfo,
            default);

        if (result != Result.Success)
        {
            submitted = false;

            throw new VulkanException(
                $"Failed to submit Vulkan command buffer: {result}");
        }

        result = Context.Api.QueueWaitIdle(
            Context.GraphicsQueue);

        if (result != Result.Success)
            throw new VulkanException(
                $"Failed waiting for Vulkan queue: {result}");

        Context.Api.FreeCommandBuffers(
            Context.LogicalDevice.Device,
            Pool,
            1,
            in commandBuffer);
    }
}
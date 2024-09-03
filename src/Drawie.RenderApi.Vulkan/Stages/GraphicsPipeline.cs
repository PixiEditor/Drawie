using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages;

public class GraphicsPipeline : IDisposable
{
    public Vk Vk { get; }
    public Device LogicalDevice { get; }
    
    public Pipeline VkPipeline => pipeline;
    public RenderPass VkRenderPass => renderPass;
    
    public PipelineLayout VkPipelineLayout => pipelineLayout;
    
    private PipelineLayout pipelineLayout;
    private Pipeline pipeline;
    private RenderPass renderPass;

    public GraphicsPipeline(Vk vk, Device logicalDevice, PipelineLayout vkPipelineLayout, Pipeline vkPipeline, RenderPass vkRenderPass)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
        
        pipelineLayout = vkPipelineLayout;
        pipeline = vkPipeline;
        renderPass = vkRenderPass;
    }
    
    public unsafe void Dispose()
    {
        Vk.DestroyPipeline(LogicalDevice, pipeline, null);
        Vk.DestroyPipelineLayout(LogicalDevice, pipelineLayout, null);
        Vk.DestroyRenderPass(LogicalDevice, renderPass, null);
    }
}
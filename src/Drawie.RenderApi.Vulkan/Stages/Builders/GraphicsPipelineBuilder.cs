using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Structs;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class GraphicsPipelineBuilder
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }
    public List<GraphicsPipelineStageBuilder> Stages { get; } = new();
    public RenderPassBuilder RenderPassBuilder { get; set; }

    public GraphicsPipelineBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public GraphicsPipelineBuilder AddStage(Action<GraphicsPipelineStageBuilder> stageBuilder)
    {
        GraphicsPipelineStageBuilder stage = new(Vk, LogicalDevice);

        stageBuilder(stage);

        Stages.Add(stage);
        return this;
    }
    
    public GraphicsPipelineBuilder WithRenderPass(Action<RenderPassBuilder> renderPassBuilder)
    {
        RenderPassBuilder = new(Vk, LogicalDevice);
        renderPassBuilder(RenderPassBuilder);
        return this;
    }

    public unsafe GraphicsPipeline Create(Extent2D swapChainExtent, Format swapChainImageFormat,
        ImageLayout finalLayout,
        ref DescriptorSetLayout descriptorSetLayout)
    {
        if (Stages.Count == 0) throw new GraphicsPipelineBuilderException("No stages were added to the pipeline.");
        if (RenderPassBuilder == null) throw new GraphicsPipelineBuilderException("No render pass was added to the pipeline.");

        RenderPass renderPass = RenderPassBuilder.Create(swapChainImageFormat, finalLayout);

        var stages = stackalloc PipelineShaderStageCreateInfo[Stages.Count];
        for (var i = 0; i < Stages.Count; i++) stages[i] = Stages[i].Build();
        
        var bindingDescription = Vertex.GetBindingDescription();
        var attributeDescriptions = Vertex.GetAttributeDescriptions();

        fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
        fixed (DescriptorSetLayout* descriptorPtr = &descriptorSetLayout)
        {

            PipelineVertexInputStateCreateInfo vertexInputInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                PVertexBindingDescriptions = &bindingDescription,
                PVertexAttributeDescriptions = attributeDescriptionsPtr
            };

            PipelineInputAssemblyStateCreateInfo inputAssembly = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false
            };

            Viewport viewport = new()
            {
                X = 0.0f,
                Y = 0.0f,
                Width = (float)swapChainExtent.Width,
                Height = (float)swapChainExtent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            Rect2D scissor = new()
            {
                Offset = new Offset2D(0, 0),
                Extent = swapChainExtent
            };

            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor
            };

            PipelineRasterizationStateCreateInfo rasterizer = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1.0f,
                CullMode = CullModeFlags.None,
                /*CullMode = CullModeFlags.BackBit,
                FrontFace = FrontFace.Clockwise,*/
                DepthBiasEnable = false
            };

            PipelineMultisampleStateCreateInfo multisampling = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.Count1Bit
            };

            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit |
                                 ColorComponentFlags.ABit,
                BlendEnable = false
            };

            PipelineColorBlendStateCreateInfo colorBlending = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachment
            };

            colorBlending.BlendConstants[0] = 0.0f;
            colorBlending.BlendConstants[1] = 0.0f;
            colorBlending.BlendConstants[2] = 0.0f;
            colorBlending.BlendConstants[3] = 0.0f;

            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                PushConstantRangeCount = 0,
                SetLayoutCount = 1,
                PSetLayouts = descriptorPtr
            };

            if (Vk!.CreatePipelineLayout(LogicalDevice, in pipelineLayoutInfo, null, out var pipelineLayout) !=
                Result.Success)
                throw new VulkanException("Failed to create pipeline layout.");

            GraphicsPipelineCreateInfo pipelineCreateInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = (uint)Stages.Count,
                PStages = stages,
                PVertexInputState = &vertexInputInfo,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisampling,
                PColorBlendState = &colorBlending,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default
            };

            if (Vk!.CreateGraphicsPipelines(LogicalDevice, default, 1, &pipelineCreateInfo, null,
                    out var graphicsPipeline) !=
                Result.Success) throw new VulkanException("Failed to create graphics pipeline.");

            foreach (var stage in Stages) stage.Dispose();

            return new GraphicsPipeline(Vk, LogicalDevice, pipelineLayout, graphicsPipeline, renderPass);
        }
    }
}
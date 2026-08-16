using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Drawie.RenderApi.Vulkan.Structs;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class GraphicsPipelineBuilder
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }
    public List<GraphicsPipelineStageBuilder> Stages { get; } = new();
    public RenderPassBuilder RenderPassBuilder { get; set; }
    public GraphicsPipelineVertexLayoutBuilder  VertexLayoutBuilder { get; set; }

    public CullModeFlags CullMode { get; set; } = CullModeFlags.None;
    public FrontFace FrontFace { get; set; } = FrontFace.Clockwise;
    public bool HasDepthStencil { get; set; }
    public PolygonMode PolygonMode { get; set; } = PolygonMode.Fill;
    public bool DoNotDisposeStages { get; set; }

    public GraphicsPipelineBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public GraphicsPipelineBuilder WithPolygonMode(PolygonMode polygonMode)
    {
        PolygonMode = polygonMode;
        return this;
    }

    public GraphicsPipelineBuilder WithCullMode(CullModeFlags cullMode)
    {
        CullMode = cullMode;
        return this;
    }

    public GraphicsPipelineBuilder WithFrontFace(FrontFace frontFace)
    {
        FrontFace = frontFace;
        return this;
    }

    public GraphicsPipelineBuilder AddStage(Action<GraphicsPipelineStageBuilder> stageBuilder)
    {
        GraphicsPipelineStageBuilder stage = new(Vk, LogicalDevice);

        stageBuilder(stage);

        Stages.Add(stage);
        return this;
    }

    public GraphicsPipelineBuilder WithVertexLayout(Action<GraphicsPipelineVertexLayoutBuilder> vertexLayoutBuilder)
    {
        GraphicsPipelineVertexLayoutBuilder builder = new GraphicsPipelineVertexLayoutBuilder();

        vertexLayoutBuilder(builder);

        VertexLayoutBuilder = builder;
        return this;
    }

    public GraphicsPipelineBuilder WithRenderPass(Action<RenderPassBuilder> renderPassBuilder)
    {
        RenderPassBuilder = new(Vk, LogicalDevice);
        renderPassBuilder(RenderPassBuilder);
        return this;
    }

    public GraphicsPipelineBuilder WithDepth()
    {
        HasDepthStencil = true;
        return this;
    }

    public unsafe GraphicsPipeline Create(Extent2D extent, Format imageFormat,
        ImageLayout finalLayout,
        ref DescriptorSetLayout descriptorSetLayout)
    {
        if (Stages.Count == 0) throw new GraphicsPipelineBuilderException("No stages were added to the pipeline.");
        if (RenderPassBuilder == null)
            throw new GraphicsPipelineBuilderException("No render pass was added to the pipeline.");
        if(VertexLayoutBuilder == null) throw new GraphicsPipelineBuilderException("No vertex layout was added to the pipeline.");

        RenderPass renderPass = RenderPassBuilder.Create(imageFormat, finalLayout);

        var stages = stackalloc PipelineShaderStageCreateInfo[Stages.Count];
        for (var i = 0; i < Stages.Count; i++) stages[i] = Stages[i].Build();

        var (bindingDescription, attributeDescriptions) = VertexLayoutBuilder.Build();

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
                Width = extent.Width,
                Height = extent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            Rect2D scissor = new()
            {
                Offset = new Offset2D(0, 0),
                Extent = extent
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
                PolygonMode = PolygonMode,
                LineWidth = 1.0f,
                CullMode = CullMode,
                FrontFace = FrontFace,
                DepthBiasEnable = false
            };

            PipelineDepthStencilStateCreateInfo depthStencil = new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,

                DepthTestEnable = HasDepthStencil,
                DepthWriteEnable = HasDepthStencil,
                DepthCompareOp = CompareOp.Less,

                DepthBoundsTestEnable = false,
                StencilTestEnable = false
            };

            PipelineMultisampleStateCreateInfo multisampling = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = FormatExtensions.ToSampleFlags(RenderPassBuilder.Samples)
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
                PDepthStencilState = &depthStencil,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default
            };

            if (Vk!.CreateGraphicsPipelines(LogicalDevice, default, 1, &pipelineCreateInfo, null,
                    out var graphicsPipeline) !=
                Result.Success) throw new VulkanException("Failed to create graphics pipeline.");

            if(!DoNotDisposeStages)
                foreach (var stage in Stages) stage.Dispose();

            return new GraphicsPipeline(Vk, LogicalDevice, pipelineLayout, graphicsPipeline, renderPass);
        }
    }
}
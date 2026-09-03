using Drawie.RenderApi.Abstraction.Pipeline;
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
    public GraphicsPipelineVertexLayoutBuilder? VertexLayoutBuilder { get; set; }

    public CullModeFlags CullMode { get; set; } = CullModeFlags.None;
    public FrontFace FrontFace { get; set; } = FrontFace.Clockwise;
    public bool HasDepthStencil { get; set; }
    public PolygonMode PolygonMode { get; set; } = PolygonMode.Fill;
    public bool BlendingEnabled { get; set; } = false;
    public bool DoNotDisposeStages { get; set; }
    public BlendFactor SrcColorBlendFactor { get; set; } = BlendFactor.SrcAlpha;
    public BlendFactor DstColorBlendFactor { get; set; } = BlendFactor.OneMinusSrcAlpha;
    public BlendOp ColorBlendOp { get; set; } = BlendOp.Add;
    public BlendFactor SrcAlphaBlendFactor { get; set; } = BlendFactor.One;
    public BlendFactor DstAlphaBlendFactor { get; set; } = BlendFactor.Zero;
    public BlendOp AlphaBlendOp { get; set; } = BlendOp.Add;
    
    public GraphicsPipelineLayoutBuilder PipelineLayoutBuilder { get; set; }


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

    public GraphicsPipelineBuilder WithBlendingPreset(BlendingPreset preset)
    {
        if (preset is BlendingPreset.Src or BlendingPreset.None)
        {
            BlendingEnabled = false;
        }
        else
        {
            BlendingEnabled = true;

            switch (preset)
            {
                case BlendingPreset.Dst:
                    SrcColorBlendFactor = BlendFactor.Zero;
                    DstColorBlendFactor = BlendFactor.One;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.Zero;
                    DstAlphaBlendFactor = BlendFactor.One;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.Normal:
                    SrcColorBlendFactor = BlendFactor.SrcAlpha;
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.One;
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.DstOver:
                    SrcColorBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstColorBlendFactor = BlendFactor.One;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstAlphaBlendFactor = BlendFactor.One;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.SrcIn:
                    SrcColorBlendFactor = BlendFactor.DstAlpha;
                    DstColorBlendFactor = BlendFactor.Zero;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.DstAlpha;
                    DstAlphaBlendFactor = BlendFactor.Zero;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.DstIn:
                    SrcColorBlendFactor = BlendFactor.Zero;
                    DstColorBlendFactor = BlendFactor.SrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.Zero;
                    DstAlphaBlendFactor = BlendFactor.SrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.SrcOut:
                    SrcColorBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstColorBlendFactor = BlendFactor.Zero;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstAlphaBlendFactor = BlendFactor.Zero;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.DstOut:
                    SrcColorBlendFactor = BlendFactor.Zero;
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.Zero;
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.SrcATop:
                    SrcColorBlendFactor = BlendFactor.DstAlpha;
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.DstAlpha;
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.DstATop:
                    SrcColorBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstColorBlendFactor = BlendFactor.SrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstAlphaBlendFactor = BlendFactor.SrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.Xor:
                    SrcColorBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.OneMinusDstAlpha;
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                case BlendingPreset.Plus:
                    SrcColorBlendFactor = BlendFactor.One;
                    DstColorBlendFactor = BlendFactor.One;
                    ColorBlendOp = BlendOp.Add;
                    SrcAlphaBlendFactor = BlendFactor.One;
                    DstAlphaBlendFactor = BlendFactor.One;
                    AlphaBlendOp = BlendOp.Add;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

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

    public GraphicsPipelineBuilder WithPipelineLayout(Action<GraphicsPipelineLayoutBuilder> layoutBuilder)
    {
        PipelineLayoutBuilder = new(Vk, LogicalDevice);
        layoutBuilder(PipelineLayoutBuilder);
        return this;
    }

    public GraphicsPipelineBuilder WithDepth()
    {
        HasDepthStencil = true;
        return this;
    }

    public unsafe GraphicsPipeline Create(Extent2D extent, Format imageFormat, ImageLayout finalLayout)
    {
        if (Stages.Count == 0) throw new GraphicsPipelineBuilderException("No stages were added to the pipeline.");
        if (RenderPassBuilder == null)
            throw new GraphicsPipelineBuilderException("No render pass was added to the pipeline.");

        RenderPass renderPass = RenderPassBuilder.Create(imageFormat, finalLayout);

        var stages = stackalloc PipelineShaderStageCreateInfo[Stages.Count];
        for (var i = 0; i < Stages.Count; i++) stages[i] = Stages[i].Build();


        PipelineVertexInputStateCreateInfo vertexInputInfo = new PipelineVertexInputStateCreateInfo()
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 0,
            VertexAttributeDescriptionCount = 0
        };

        if (VertexLayoutBuilder != null)
        {
            var (bindingDescription, attributeDescriptions) = VertexLayoutBuilder.Build();
            fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
            {
                vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                    PVertexBindingDescriptions = &bindingDescription,
                    PVertexAttributeDescriptions = attributeDescriptionsPtr,
                };
            }
        }

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
            BlendEnable = BlendingEnabled,
            SrcColorBlendFactor = SrcColorBlendFactor,
            DstColorBlendFactor = DstColorBlendFactor,
            ColorBlendOp = ColorBlendOp,
            SrcAlphaBlendFactor = SrcAlphaBlendFactor,
            DstAlphaBlendFactor = DstAlphaBlendFactor,
            AlphaBlendOp = AlphaBlendOp
        };

        PipelineColorBlendStateCreateInfo colorBlending = new()
        {
            LogicOpEnable = false,
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOp = LogicOp.Copy,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment
        };

        colorBlending.BlendConstants[0] = 0.0f;
        colorBlending.BlendConstants[1] = 0.0f;
        colorBlending.BlendConstants[2] = 0.0f;
        colorBlending.BlendConstants[3] = 0.0f;

        if(PipelineLayoutBuilder == null)
            throw new GraphicsPipelineBuilderException("No pipeline layout was added to the pipeline.");
        
        var pipelineLayout = PipelineLayoutBuilder.Create();

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

        if (!DoNotDisposeStages)
            foreach (var stage in Stages)
                stage.Dispose();
        
        return new GraphicsPipeline(Vk, LogicalDevice, pipelineLayout, graphicsPipeline, renderPass);
    }
}
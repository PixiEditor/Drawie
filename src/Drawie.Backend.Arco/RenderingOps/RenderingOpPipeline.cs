using Drawie.Backend.Arco.Blending;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Core;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.Backend.Arco.RenderingOps;

public class RenderingOpPipeline
{
    public IGraphicsDevice GraphicsDevice { get; }
    public IShaderProgram ShaderProgram { get; }
    public Dictionary<BlendMode, IPipeline> BlendModePipelines { get; } = new Dictionary<BlendMode, IPipeline>();
    public Guid PipelineGroupId { get; } = Guid.NewGuid();

    public RenderingOpPipeline(IGraphicsDevice device, Shader vertex, Shader fragment)
    {
        GraphicsDevice = device;
        ShaderProgram = GraphicsDevice.CreateShaderProgram(new ShaderProgramDesc([vertex, fragment]));

        CreatePipelineForBlendMode( BlendMode.SrcOver);
    }

    private void CreatePipelineForBlendMode(BlendMode blendMode)
    {
        BlendModePipelines[blendMode] = GraphicsDevice.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = false,
            },
            Rasterizer = new RasterizerDesc()
            {
                RenderMode = RenderMode.Default,
                Samples = 1,
                CullMode = CullMode.None
            },
            Blend = new BlendDesc()
            {
                Preset = blendMode.ToBlendingPreset()
            },
            RenderPass = new RenderPassDesc()
            {
                ColorLoadOp = ColorLoadOp.Load
            },
            DynamicViewport = true,
            ShaderProgram = ShaderProgram,
            PipelineVariantGroupId = PipelineGroupId
        });
    }

    public IPipeline GetPipelineFor(BlendMode blendMode)
    {
        if (!BlendModePipelines.ContainsKey(blendMode))
        {
            CreatePipelineForBlendMode(blendMode);
        }

        return BlendModePipelines[blendMode];
    }
}

public enum RenderOpType
{
    Rect,
    Circle
}
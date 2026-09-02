using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public struct PipelineDesc : IEquatable<PipelineDesc>
{
    public IShaderProgram? ShaderProgram { get; set; }
    public RenderPassDesc RenderPass { get; set; }
    public DepthDesc Depth { get; set; }
    public BlendDesc  Blend { get; set; }
    public RasterizerDesc Rasterizer { get; set; }
    public RectI Viewport { get; set; }
    
    public bool Equals(PipelineDesc other)
    {
        return Equals(ShaderProgram, other.ShaderProgram) && Depth.Equals(other.Depth) && Blend.Equals(other.Blend) && Rasterizer.Equals(other.Rasterizer) && Viewport.Equals(other.Viewport);
    }

    public override bool Equals(object? obj)
    {
        return obj is PipelineDesc other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ShaderProgram, Depth, Blend, Rasterizer, Viewport);
    }
}

public record struct DepthDesc
{
    public bool Enabled { get; set; }
    public DepthCompareType DepthCompare  { get; set; }
    public DepthFormat Format { get; set; }
}

public enum DepthCompareType
{
    Never,
    Less,
    LessEqual,
    Equal,
    Greater,
    GreaterEqual,
    NotEqual,
    Always
}

public record struct BlendDesc
{
    public BlendingPreset Preset { get; set; }
}
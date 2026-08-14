using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public struct PipelineDesc
{
    public IShaderProgram? ShaderProgram { get; set; }
    public DepthDesc Depth { get; set; }
    public BlendDesc  Blend { get; set; }
    public RasterizerDesc Rasterizer { get; set; }
    public RectI Viewport { get; set; }
}

public struct DepthDesc
{
    public bool Enabled { get; set; }
    public DepthCompareType DepthCompare  { get; set; }
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

public struct BlendDesc
{
    public bool Enabled;
}
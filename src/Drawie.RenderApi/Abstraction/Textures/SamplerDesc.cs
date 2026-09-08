using Drawie.RenderApi.Abstraction.Pipeline;

namespace Drawie.RenderApi.Abstraction.Textures;

public struct SamplerDesc
{
    public FilterMode MinFilter { get; set; }
    public FilterMode MagFilter { get; set; }
    
    public EdgeWrapMode WrapU { get; set; }
    public EdgeWrapMode WrapV { get; set; }
    public EdgeWrapMode WrapW { get; set; }
    
    public bool EnableCompare { get; set; }
    public DepthCompareType CompareOp { get; set; }
    
    public bool UnnormalizedCoordinates { get; set; }
}

public enum FilterMode
{
    Nearest,
    Linear,
}

public enum EdgeWrapMode
{
    Repeat = 0,
    MirroredRepeat = 1,
    ClampToEdge = 2,
    ClampToBorder = 3,
    MirrorClampToEdge = 4,
}
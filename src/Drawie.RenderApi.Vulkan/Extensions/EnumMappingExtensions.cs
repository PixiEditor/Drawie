using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Extensions;

public static class EnumMappingExtensions
{
    public static Filter ToFilter(this FilterMode filterMode)
    {
        return filterMode switch
        {
            FilterMode.Nearest => Filter.Nearest,
            FilterMode.Linear => Filter.Linear,
            _ => throw new ArgumentOutOfRangeException(nameof(filterMode), filterMode, null)
        };
    }

    public static SamplerAddressMode ToAddressMode(this EdgeWrapMode wrapMode)
    {
        return wrapMode switch
        {
            EdgeWrapMode.Repeat => SamplerAddressMode.Repeat,
            EdgeWrapMode.MirroredRepeat => SamplerAddressMode.MirroredRepeat,
            EdgeWrapMode.ClampToEdge => SamplerAddressMode.ClampToEdge,
            EdgeWrapMode.ClampToBorder => SamplerAddressMode.ClampToBorder,
            EdgeWrapMode.MirrorClampToEdge => SamplerAddressMode.MirrorClampToEdge,
            _ => throw new ArgumentOutOfRangeException(nameof(wrapMode), wrapMode, null)
        };
    }

    public static CompareOp ToCompareOp(this DepthCompareType compareOp)
    {
        return compareOp switch
        {
            DepthCompareType.Never => CompareOp.Never,
            DepthCompareType.Less => CompareOp.Less,
            DepthCompareType.LessEqual => CompareOp.LessOrEqual,
            DepthCompareType.Equal => CompareOp.Equal,
            DepthCompareType.Greater => CompareOp.Greater,
            DepthCompareType.GreaterEqual => CompareOp.GreaterOrEqual,
            DepthCompareType.NotEqual => CompareOp.NotEqual,
            DepthCompareType.Always => CompareOp.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(compareOp), compareOp, null)
        };
    }
}
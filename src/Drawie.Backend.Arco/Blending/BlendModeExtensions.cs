using Drawie.Backend.Core.Surfaces;
using Drawie.RenderApi.Abstraction.Pipeline;

namespace Drawie.Backend.Arco.Blending;

public static class BlendModeExtensions
{
    public static BlendingPreset ToBlendingPreset(this BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Src => BlendingPreset.Src,
            BlendMode.Dst => BlendingPreset.Dst,
            BlendMode.SrcOver => BlendingPreset.Normal,
            BlendMode.DstOver => BlendingPreset.DstOver,
            BlendMode.SrcIn => BlendingPreset.SrcIn,
            BlendMode.DstIn => BlendingPreset.DstIn,
            BlendMode.SrcOut => BlendingPreset.SrcOut,
            BlendMode.DstOut => BlendingPreset.DstOut,
            BlendMode.SrcATop => BlendingPreset.SrcATop,
            BlendMode.DstATop => BlendingPreset.DstATop,
            BlendMode.Xor => BlendingPreset.Xor,
            BlendMode.Plus => BlendingPreset.Plus,
            _ => throw new NotSupportedException($"{mode} is not a valid, hardware-supported blending mode."),
        };
    }
}
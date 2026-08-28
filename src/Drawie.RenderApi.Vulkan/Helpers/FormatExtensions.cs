using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Helpers;

public static class FormatExtensions
{
    public static Format ToVkFormat(this DepthFormat descDepth)
    {
        switch (descDepth)
        {
            case DepthFormat.NoDepth:   
                throw new ArgumentException("No depth is not supported depth format.");
            case DepthFormat.Depth24Stencil8:
                return Format.D24UnormS8Uint;
            default:
                throw new ArgumentOutOfRangeException(nameof(descDepth), descDepth, null);
        }
    }
    
    public static SampleCountFlags ToSampleFlags(int samples)
    {
        return samples switch
        {
            1 => SampleCountFlags.Count1Bit,
            2 => SampleCountFlags.Count2Bit,
            4 => SampleCountFlags.Count4Bit,
            8 => SampleCountFlags.Count8Bit,
            16 => SampleCountFlags.Count16Bit,
            32 => SampleCountFlags.Count32Bit,
            64 => SampleCountFlags.Count64Bit,
            _ => throw new ArgumentOutOfRangeException(nameof(samples), samples, null)
        };
    }
}
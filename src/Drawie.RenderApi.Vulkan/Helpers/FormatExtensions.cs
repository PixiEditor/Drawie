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
}
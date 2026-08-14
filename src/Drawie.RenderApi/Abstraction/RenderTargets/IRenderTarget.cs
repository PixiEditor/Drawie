using Drawie.Numerics;

namespace Drawie.RenderApi.Abstraction.RenderTargets;

public interface IRenderTarget
{
    ulong FramebufferId { get; }
    VecI Size { get; }
}
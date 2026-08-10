using Drawie.Numerics;

namespace Drawie.RenderApi.Abstraction.RenderTargets;

public interface IRenderTarget
{
    uint FramebufferId { get; }
    VecI Size { get; }
}
using Drawie.Numerics;

namespace Drawie.RenderApi.Abstraction.RenderTargets;

public interface IRenderTarget
{
    ulong SurfaceId { get; }
    VecI Size { get; }
}
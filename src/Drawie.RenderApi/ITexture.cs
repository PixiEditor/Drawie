using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface ITexture : IAsyncDisposable
{
    public VecI Size { get; }
    public void BlitFrom(ITexture texture);
    public void BlitFrom(ITexture backingBackbufferTexture, object? renderFinishedSemaphore,
        object? blitSignalSemaphore);
}

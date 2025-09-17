using Avalonia.Platform;

namespace Drawie.Interop.Avalonia.Core;

public interface ISemaphorePair : IDisposable
{
    public IPlatformHandle Export(bool renderFinished);
    public object RenderFinishedSemaphore { get; }
    public object AvailableSemaphore { get; }
}

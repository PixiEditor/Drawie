using Avalonia.Platform;
using Drawie.Numerics;

namespace Drawie.Interop.Avalonia.Core;

public struct FrameHandle
{
    public IPlatformHandle ImageHandle { get; set; }
    public IPlatformHandle AvailableSemaphore { get; set; }
    public IPlatformHandle RenderCompletedSemaphore { get; set; }
    public ulong MemorySize { get; set; }
    public VecI Size { get; set; }
}

using Avalonia.Platform;

namespace Drawie.Interop.Avalonia.Core;

public interface IExportable
{
    public IPlatformHandle Export();
    public ulong MemorySize { get; }
}

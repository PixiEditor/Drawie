using Avalonia.Platform;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public interface IExportableTexture : ITexture
{
    public IPlatformHandle Export();
    public ulong MemorySize { get; }
    public void PrepareForImport(object waitSemaphore, object signalSemaphore);
}

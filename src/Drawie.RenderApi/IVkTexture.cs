using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface IVkTexture : ITexture, IDisposable
{
   public uint QueueFamily { get; }
   public uint ImageFormat { get; }
   public ulong ImageHandle { get; }
   public uint UsageFlags { get; }
   public uint Layout { get; }
   public uint TargetSharingMode { get; }
   public uint Tiling { get; }
   ulong MemorySize { get; set; }
   public VecI Size { get; }
   public void MakeReadOnly();
   public void MakeWriteable();
   public (IntPtr handle, string? descriptor) Export();
}

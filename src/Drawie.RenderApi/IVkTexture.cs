namespace Drawie.RenderApi;

public interface IVkTexture : ITexture
{
   public uint QueueFamily { get; }
   public uint ImageFormat { get; }
   public ulong ImageHandle { get; }
   public uint UsageFlags { get; }
   public uint Layout { get; }
   public uint TargetSharingMode { get; }
   public uint Tiling { get; }
   public void MakeReadOnly();
   public void MakeWriteable();
}
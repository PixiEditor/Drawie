namespace Drawie.RenderApi;

public interface IVulkanContextInfo
{
    public string[] GetRequiredExtensions();
    public ulong GetSurfaceHandle(IntPtr instanceHandle);
    public bool HasSurface { get; }
}
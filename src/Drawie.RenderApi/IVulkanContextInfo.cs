namespace Drawie.RenderApi;

public interface IVulkanContextInfo
{
    public string[] GetInstanceExtensions();
    public ulong GetSurfaceHandle(IntPtr instanceHandle);
    public bool HasSurface { get; }
}
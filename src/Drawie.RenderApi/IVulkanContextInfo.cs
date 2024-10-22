namespace Drawie.RenderApi;

public interface IVulkanContextInfo
{
    public string[] GetRequiredExtensions();
    public ulong GetSurfaceHandle(IntPtr instanceHandle);
}
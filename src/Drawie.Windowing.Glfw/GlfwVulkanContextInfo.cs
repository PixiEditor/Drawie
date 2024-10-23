using Drawie.RenderApi;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Drawie.Silk;

public class GlfwVulkanContextInfo(IVkSurface WindowVkSurface) : IVulkanContextInfo
{
    public unsafe string[] GetInstanceExtensions()
    {
        var windowExtensions = WindowVkSurface.GetRequiredExtensions(out var count);
        var extensions = SilkMarshal.PtrToStringArray((nint)windowExtensions, (int)count);
        
        return extensions;
    }

    public unsafe ulong GetSurfaceHandle(IntPtr instanceHandle)
    {
        return WindowVkSurface!.Create<AllocationCallbacks>(new VkHandle(instanceHandle), null).Handle;
    }

    public bool HasSurface => WindowVkSurface != null;
}
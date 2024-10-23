namespace Drawie.RenderApi;

public interface IVulkanContext
{
    public IntPtr LogicalDeviceHandle { get; }
    public IntPtr InstanceHandle { get; }
    public IntPtr PhysicalDeviceHandle { get; }
    public IntPtr GraphicsQueueHandle { get; }
    public uint GraphicsQueueFamilyIndex { get; }
    public IntPtr GetProcedureAddress(string name, IntPtr instance, IntPtr device);
}
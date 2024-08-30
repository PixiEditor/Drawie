using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public struct SwapChainSupportDetails
{
   public SurfaceCapabilitiesKHR Capabilities { get; set; }
   public SurfaceFormatKHR[] Formats { get; set; }
   public PresentModeKHR[] PresentModes { get; set; }
}
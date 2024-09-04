using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Helpers;

public static class ImageUtility
{
    public static unsafe ImageView CreateViewForImage(Vk vk, Device logicalDevice, Image image, Format format)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = format,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        if (vk!.CreateImageView(logicalDevice, in createInfo, null, out var view) != Result.Success)
            throw new VulkanException("Failed to create image views.");
        
        return view;
    }
}
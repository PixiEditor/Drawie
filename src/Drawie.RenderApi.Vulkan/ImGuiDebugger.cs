using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using ImGuiNET;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class ImGuiDebugger : IDisposable
{
    private readonly VulkanWindowRenderApi renderApi;
    
    private DescriptorPool _descriptorPool;
    private readonly DescriptorSetLayout _descriptorSetLayout;
    private readonly VulkanTexture _fontTexture;
    
    public ImGuiDebugger(VulkanWindowRenderApi renderApi)
    {
        this.renderApi = renderApi;
    }

    public unsafe void Attach()
    {
        var ctx = ImGui.CreateContext();
        ImGui.SetCurrentContext(ctx);
        
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);
        
        CreateDescriptorPool();
        // TODO: make a good abstraction for vulkan stuff first
        /*_fontTexture = new VulkanTexture(renderApi, width, height, Format.R8G8B8A8Unorm,
            ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit, pixels);*/
    }

    private unsafe void CreateDescriptorPool()
    {
        DescriptorPoolSize descriptorPoolSize = new DescriptorPoolSize
        {
            Type = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1000
        };
        
        DescriptorPoolCreateInfo descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
        {
            MaxSets = 100,
            PPoolSizes = &descriptorPoolSize,
        };
        
        renderApi.Vk.CreateDescriptorPool(renderApi.LogicalDevice, &descriptorPoolCreateInfo, null, out _descriptorPool);
    }


    public void Dispose()
    {
        
    }
}
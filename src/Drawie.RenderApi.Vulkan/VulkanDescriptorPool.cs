using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanDescriptorPool
{
    public VulkanContext Context { get; }
    public DescriptorPool DescriptorPool { get; }
    
    public DescriptorSetLayout[] DescriptorSetLayouts { get; }

    private Dictionary<ulong, DescriptorSet> sets = new Dictionary<ulong, DescriptorSet>();

    public VulkanDescriptorPool(VulkanContext context, DescriptorPool descriptorPool,
        DescriptorSetLayout[] descriptorSetLayouts)
    {
        Context = context;
        DescriptorPool = descriptorPool;
        DescriptorSetLayouts = descriptorSetLayouts;
    }
    
    public void Reset()
    {
        sets.Clear();
        
        Context.Api.ResetDescriptorPool(
            Context.LogicalDevice.Device,
            DescriptorPool,
            0);
    }
    
    public unsafe DescriptorSet GetOrAllocateDescriptorSet(int setIndex, ulong forHandle)
    {
        if (sets.TryGetValue(forHandle, out var set)) return set;
        
        DescriptorSetLayout layout = DescriptorSetLayouts[setIndex];

        DescriptorSetAllocateInfo allocInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = DescriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &layout
        };

        DescriptorSet descriptorSet;

        var result = Context.Api.AllocateDescriptorSets(
            Context.LogicalDevice.Device,
            in allocInfo,
            &descriptorSet);

        if (result != Result.Success)
            throw new VulkanException(
                $"Failed to allocate descriptor set: {result}");

        sets[forHandle] = descriptorSet;
        return descriptorSet;
    }

    public unsafe void Dispose()
    {
        Context.Api.DestroyDescriptorPool(Context.LogicalDevice.Device, DescriptorPool, null);
    }
}
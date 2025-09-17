using Silk.NET.Vulkan;
using Semaphore = System.Threading.Semaphore;

namespace Drawie.RenderApi.Vulkan.Helpers;

public class SingleTimeCommandBufferSession : IDisposable
{
    public CommandPool CommandPool { get; set; }
    public Device LogicalDevice { get; set; }
    public Vk Vk { get; set; }
    
    public CommandBuffer CommandBuffer { get; private set; } 
    
    private Queue graphicsQueue;
    
    
    public SingleTimeCommandBufferSession(Vk vk, CommandPool pool, Device device, Queue graphicsQueue)
    {
        CommandPool = pool;
        LogicalDevice = device;
        Vk = vk;
        this.graphicsQueue = graphicsQueue;
        
        Begin();
    }
    
    public SingleTimeCommandBufferSession(VulkanContext context, CommandPool pool) : this(context.Api!, pool, context.LogicalDevice.Device, context.GraphicsQueue)
    {
    }
        
    
    private void Begin()
    {
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };
        
        Vk.AllocateCommandBuffers(LogicalDevice, allocInfo, out var commandBuffer);
        
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        
        Vk.BeginCommandBuffer(commandBuffer, beginInfo);
        CommandBuffer = commandBuffer;
    }

    public unsafe void End(Silk.NET.Vulkan.Semaphore? signalSemaphore = null)
    {
        Vk.EndCommandBuffer(CommandBuffer);
        
        Silk.NET.Vulkan.CommandBuffer commandBuffer = CommandBuffer;
        
        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
        };

        if (signalSemaphore != null)
        {
            var semaphore = signalSemaphore.Value;
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = &semaphore;
        }
        
        Vk.QueueSubmit(graphicsQueue, 1, submitInfo, default);
        Vk.QueueWaitIdle(graphicsQueue);
        
        Vk.FreeCommandBuffers(LogicalDevice, CommandPool, 1, &commandBuffer);
    }
    
    public void Dispose()
    {
        End();
    }
}

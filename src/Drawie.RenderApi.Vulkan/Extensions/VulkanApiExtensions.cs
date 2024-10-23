using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Extensions;

public static class VulkanApiExtensions
{
    public static void ThrowOnError(this Result result, string message)
    {
        if (result != Result.Success) throw new VulkanException($"{message}: \"{result}\".");
    }
}
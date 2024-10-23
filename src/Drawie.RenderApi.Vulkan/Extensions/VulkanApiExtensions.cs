using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Extensions;

public static class VulkanApiExtensions
{
    public static void ThrowOnError(this Result result, string? message = "")
    {
        message ??= "Vulkan API call failed";
        if (result != Result.Success) throw new VulkanException($"{message}: \"{result}\".");
    }
}
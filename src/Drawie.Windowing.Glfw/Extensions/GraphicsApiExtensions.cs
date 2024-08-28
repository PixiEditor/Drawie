using Drawie.RenderApi;
using Silk.NET.Windowing;

namespace Drawie.Silk.Extensions;

public static class GraphicsApiExtensions
{
    public static GraphicsAPI ToSilkGraphicsApi(this GraphicsApi api)
    {
        return api switch
        {
            GraphicsApi.Vulkan => GraphicsAPI.DefaultVulkan,
            _ => GraphicsAPI.None
        };
    }
}
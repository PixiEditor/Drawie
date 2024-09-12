using Drawie.RenderApi;

namespace Drawie.Skia.Exceptions;

public class UnsupportedRenderApiException : Exception
{
    public UnsupportedRenderApiException(IRenderApi api) : base($"Render API {api.GetType().Name} is not supported.")
    {
    }
}
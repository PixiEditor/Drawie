namespace Drawie.RenderApi.Abstraction.Textures;

public struct PreparedTexture
{
    public ulong Handle { get; }
    public PreparedTexture(ulong handle)
    {
        Handle = handle;
    }
}
namespace Drawie.RenderApi.Abstraction.Textures;

public struct PreparedTexture
{
    public string Name { get; set; }
    public ulong Handle { get; }
    public bool IsPartOfArray { get; set; }

    public PreparedTexture(string name, ulong handle)
    {
        Name = name;
        Handle = handle;
    }
}
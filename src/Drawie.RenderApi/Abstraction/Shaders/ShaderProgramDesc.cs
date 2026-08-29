using Drawie.Backend.Shaders.Common;

namespace Drawie.RenderApi.Abstraction.Shaders;

public struct ShaderProgramDesc
{
    public List<ShaderDesc> Shaders { get; }

    public ShaderProgramDesc(IEnumerable<ShaderDesc> desc)
    {
        Shaders = new List<ShaderDesc>(desc);
    }
}

public struct ShaderDesc
{
    public string EntryName { get; set; }
    public byte[] Bytes { get; }
    public ShaderType Type { get; }

    public ShaderDesc(string entryName, byte[] bytes, ShaderType type)
    {
        EntryName = entryName;
        Bytes = bytes;
        Type = type;
    }

    public ShaderDesc(Shader shader)
    {
        EntryName = shader.EntryName;
        Bytes = shader.ShaderBytes;
        Type = shader.ShaderType;
    }
}
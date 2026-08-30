using Drawie.Backend.Shaders.Common;

namespace Drawie.RenderApi.Abstraction.Shaders;

public struct ShaderProgramDesc
{
    public List<Shader> Shaders { get; }

    public ShaderProgramDesc(IEnumerable<Shader> desc)
    {
        Shaders = new List<Shader>(desc);
    }
}
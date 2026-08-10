using Drawie.RenderApi.Abstraction.Shaders;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlShaderProgram : IShaderProgram
{
    public uint ProgramHandle { get; }
    public GL Api { get; }

    public OpenGlShaderProgram(GL gl, uint programHandle)
    {
        ProgramHandle = programHandle;
        Api = gl;
    }

    public void Use()
    {
        Api.UseProgram(ProgramHandle);
    }
}
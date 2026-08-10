namespace Drawie.RenderApi.Abstraction.Shaders;

public struct ShaderProgramDesc
{
    public byte[] VertexShaderBytes { get; }
    public byte[] FragmentShaderBytes { get; }

    public ShaderProgramDesc(byte[] vertexShaderBytes, byte[] fragmentShaderBytes)
    {
        VertexShaderBytes = vertexShaderBytes;
        FragmentShaderBytes = fragmentShaderBytes;
    }
}
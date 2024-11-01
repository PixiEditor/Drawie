namespace Drawie.RenderApi.WebGl.Exceptions;

public class ShaderCompilationException : WebGlException
{
    public ShaderCompilationException(WebGlShaderType webGlShaderType, string shaderSource, string compilationLog)
        : base($"Failed to compile {webGlShaderType} shader. Shader source: {shaderSource}. Compilation log: {compilationLog}")
    {
    }
}

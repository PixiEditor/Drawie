namespace Drawie.Backend.Core.Exceptions;

public class ShaderCompilationException : Exception
{
    public ShaderCompilationException(string errors, string sksl) : base($"Shader compilation failed: {errors}\n\n{sksl}")
    {
    }
}

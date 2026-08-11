using System.Numerics;
using Slangc.NET;

namespace Drawie.Backend.Vertie.Rendering;

public sealed class ShaderDefinition
{
    public string SourceCode { get; }
    private Guid id = Guid.NewGuid();

    public ShaderDefinition(string slangCode)
    {
        SourceCode = slangCode;
    }

    public CompiledShader Compile()
    {
        var bytes = SlangCompiler.CompileWithReflection(SourceCode,
        [
            "-profile", "glsl_450",
            "-matrix-layout-row-major",
            "-fvk-use-entrypoint-name",
            "-target", "spirv"
        ], out var reflection);

        return new CompiledShader(bytes, reflection);
    }
}
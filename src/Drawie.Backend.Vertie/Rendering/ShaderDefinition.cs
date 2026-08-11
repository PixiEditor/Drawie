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
        var vertex = SlangCompiler.CompileWithReflection(SourceCode,
        [
            "-profile", "glsl_450",
            "-matrix-layout-row-major",
            "-fvk-use-entrypoint-name",
            "-entry", "VSMain",
            "-stage", "vertex",
            "-target", "spirv"
        ], out var vertexReflection);

        var fragment = SlangCompiler.CompileWithReflection(SourceCode,
        [
            "-profile", "glsl_450",
            "-matrix-layout-row-major",
            "-fvk-use-entrypoint-name",
            "-entry", "FSMain",
            "-stage", "fragment",
            "-target", "spirv"
        ], out var fragmentReflection);
        
        return new CompiledShader(vertex, fragment, vertexReflection, fragmentReflection);
    }
}
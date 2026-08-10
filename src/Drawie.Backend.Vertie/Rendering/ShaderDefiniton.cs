using System.Numerics;
using Slangc.NET;

namespace Drawie.Backend.Vertie.Rendering;

public sealed class ShaderDefiniton
{
    public string SourceCode { get; }
    private Guid id = Guid.NewGuid();

    public ShaderDefiniton(string slangCode)
    {
        SourceCode = slangCode;
    }

    public CompiledShader Compile()
    {
        var vertex = SlangCompiler.Compile(SourceCode,
        [
            "-profile", "glsl_450",
            "-matrix-layout-row-major",
            "-fvk-use-entrypoint-name",
            "-entry", "VSMain",
            "-stage", "vertex",
            "-target", "spirv"
        ]);

        var fragment = SlangCompiler.Compile(SourceCode,
        [
            "-profile", "glsl_450",
            "-matrix-layout-row-major",
            "-fvk-use-entrypoint-name",
            "-entry", "FSMain",
            "-stage", "fragment",
            "-target", "spirv"
        ]);
        
        return new CompiledShader(vertex, fragment);
    }
}
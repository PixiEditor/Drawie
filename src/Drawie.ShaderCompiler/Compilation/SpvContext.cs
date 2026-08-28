using Drawie.ShaderCompiler.Compilation;
using Silk.NET.SPIRV.Cross;

namespace Drawie.Backend.Vertie.Compilation;

public class SpvContext
{
    private unsafe Context* spvContext;
    private Cross api;

    public unsafe SpvContext(Cross api)
    {
        this.api = api;
        Context* context = null;
        
        var result = api.ContextCreate(&context);
        if (result != Result.Success)
        {
            throw new Exception($"SPIRV-Cross context creation failed: {result}");
        }

        spvContext = context;
    }

    public unsafe SpvCompiler CreateCompiler(Silk.NET.SPIRV.Cross.Backend backend, byte[] spirv)
    {
        return new SpvCompiler(api, spvContext, backend, spirv);
    }
}
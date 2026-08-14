using System.Runtime.InteropServices;
using Drawie.Backend.Vertie.Compilation;
using Silk.NET.SPIRV.Cross;

namespace Drawie.ShaderCompiler.Compilation;

public class SpvCompiler
{
    private unsafe Compiler* compiler;
    private unsafe CompilerOptions* options;

    private Cross api;

    public unsafe SpvCompiler(Cross api, Context* spvContext, Silk.NET.SPIRV.Cross.Backend backend, byte[] spirv)
    {
        this.api = api;
        Compiler* crossCompiler = null;
        Compiler** crossCompilerPtr = &crossCompiler;

        if (spirv.Length == 0)
            throw new ArgumentException(
                "SPIR-V cannot be empty.",
                nameof(spirv));

        if (spirv.Length % sizeof(uint) != 0)
            throw new ArgumentException(
                "SPIR-V byte length must be a multiple of 4.",
                nameof(spirv));


        ParsedIr* parsedIr = null;

        var words = MemoryMarshal.Cast<byte, uint>(spirv);

        var parseResult = api.ContextParseSpirv(
            spvContext,
            words,
            (nuint)words.Length,
            &parsedIr);

        if (parseResult != Result.Success)
            throw new Exception(
                $"SPIR-V parsing failed: {parseResult}");

        var result = api.ContextCreateCompiler(spvContext, backend, parsedIr,
            CaptureMode.TakeOwnership, crossCompilerPtr);

        if (result != Result.Success)
            throw new Exception($"GLSL compiler creation failed: {result}");

        compiler = crossCompiler;

        CompilerOptions* options = null;
        var optionsResult = api.CompilerCreateCompilerOptions(crossCompiler, &options);

        if (optionsResult != Result.Success)
            throw new Exception($"Failed to create compiler options: {optionsResult}");

        this.options = options;
    }

    public unsafe void ConfigureGlslVersion(GlslVersion version)
    {
        api.CompilerOptionsSetUint(options, CompilerOption.GlslVersion, (uint)version);
        api.CompilerOptionsSetBool(options, CompilerOption.GlslES, 1);
    }

    public unsafe string Compile()
    {
        var installResult = api.CompilerInstallCompilerOptions(compiler, options);
        if (installResult != Result.Success)
        {
            throw new Exception($"Failed to install compiler options: {installResult}");
        }

        byte* source = null;

        var result = api.CompilerCompile(compiler, &source);
        if (result != Result.Success)
        {
            throw new Exception($"Compilation failed: {result}");
        }

        string code = Marshal.PtrToStringUTF8((nint)source)!;
        return code;
    }
}
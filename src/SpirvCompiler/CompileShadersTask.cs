using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using shaderc;
using Task = Microsoft.Build.Utilities.Task;

namespace SpirvCompiler;

public class CompileShadersTask : Task
{
    [Required]
    public string ShadersPath { get; set; } = string.Empty;

    [Required] 
    public string OutputPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, "Compiling shaders...");

        if (string.IsNullOrEmpty(ShadersPath))
        {
            Log.LogError("ShadersPath is not set.");
            return false;
        }

        if (string.IsNullOrEmpty(OutputPath))
        {
            Log.LogError("OutputPath is not set.");
            return false;
        }

        string vertexShaderPath = Path.Combine(ShadersPath, "basic.vert");
        string fragmentShaderPath = Path.Combine(ShadersPath, "basic.frag");

        Compiler compiler = new();
        using Result res = compiler.Compile(vertexShaderPath, ShaderKind.VertexShader);
        if (res.Status != Status.Success)
        {
            Log.LogError($"Failed to compile vertex shader: {res.ErrorMessage}");
            return false;
        }

        byte[] vertexShaderBytes = ResultToBytes(res);

        using Result res2 = compiler.Compile(fragmentShaderPath, ShaderKind.FragmentShader);
        if (res2.Status != Status.Success)
        {
            Log.LogError($"Failed to compile fragment shader: {res2.ErrorMessage}");
            return false;
        }

        byte[] fragmentShaderBytes = ResultToBytes(res2);

        string outputVertexPath = Path.Combine(OutputPath, "vert.spv");
        string outputFragmentPath = Path.Combine(OutputPath, "frag.spv");

        File.WriteAllBytes(outputVertexPath, vertexShaderBytes);
        File.WriteAllBytes(outputFragmentPath, fragmentShaderBytes);
        
        compiler.Dispose();
        
        Log.LogMessage(MessageImportance.High, "Shaders compiled successfully.");

        return true;
    }

    private static byte[] ResultToBytes(Result result)
    {
        var codePtr = result.CodePointer;
        var codeLength = result.CodeLength;

        byte[] code = new byte[codeLength];
        Marshal.Copy(codePtr, code, 0, (int)codeLength);
        return code;
    }
}

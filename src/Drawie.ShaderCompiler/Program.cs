using System.Runtime.InteropServices;
using Drawie.ShaderCompiler.Compilation;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace Drawie.ShaderCompiler;

public sealed class ShaderCompilerTask : Task
{
    [Required] public string OutputDirectory { get; set; } = "";

    [Required] public string ShaderRoot { get; set; } = "";

    public bool Browser { get; set; }

    public override bool Execute()
    {
        try
        {
            Directory.CreateDirectory(OutputDirectory);

            foreach (var source in Directory.GetFiles(ShaderRoot, "*.slang", SearchOption.AllDirectories))
            {
                try
                {
                    CompileShader(source);
                }
                catch (Exception ex)
                {
                    Log.LogError(
                        $"Failed to compile shader '{source}': {ex}");

                    return false;
                }
            }

            return !Log.HasLoggedErrors;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }

    private void CompileShader(string sourcePath)
    {
        var target = Browser
            ? CompilationTarget.GlslEs3
            : CompilationTarget.SpirV;
        
        Log.LogMessage(
            MessageImportance.High,
            $"Compiling shader: {sourcePath}, target: {target}");

        var sourceCode = File.ReadAllText(sourcePath);


        Compilation.ShaderCompiler compiler =
            new Compilation.ShaderCompiler(OutputDirectory, Path.GetFileNameWithoutExtension(sourcePath));
        compiler.Compile(sourceCode, target);

        Log.LogMessage(
            MessageImportance.High,
            $"Generated: {Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".shader")} and {Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".reflection.json")}");
    }
}
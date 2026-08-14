using System.Text;
using System.Text.Json;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Compilation;
using Slangc.NET;

namespace Drawie.ShaderCompiler.Compilation;

public class ShaderCompiler
{
    public string OutputPath { get; set; }
    public string ShaderName { get; set; }

    public ShaderCompiler(string outputPath, string shaderName)
    {
        OutputPath = outputPath;
        ShaderName = shaderName;
    }

    public void Compile(string sourceCode, CompilationTarget target)
    {
        var bytes = SlangCompiler.CompileWithReflection(
            sourceCode,
            [
                "-matrix-layout-column-major",
                "-fvk-use-entrypoint-name",
                "-emit-spirv-via-glsl",
                "-profile", "glsl_330",
            ],
            out var reflection);
        
        File.WriteAllBytes(Path.Combine(OutputPath, $"{ShaderName}.spv"), bytes);
        
        if (target == CompilationTarget.GlslEs3)
        {
            var compiler = new CrossCompiler();

            var glslCode = compiler.CompileToGlslEs3(bytes);

            bytes = Encoding.UTF8.GetBytes(glslCode);
        }

        var outputName =
            Path.GetFileNameWithoutExtension(ShaderName) + ".shader";

        var outputReflectionName = Path.GetFileNameWithoutExtension(ShaderName) + ".reflection.json";

        var outputPath =
            Path.Combine(OutputPath, outputName);

        var outputReflectionPath = Path.Combine(OutputPath, outputReflectionName);

        File.WriteAllBytes(outputPath, bytes);
        File.WriteAllText(outputReflectionPath, ToReflectionJson(reflection));
    }


    private string ToReflectionJson(SlangReflection reflection)
    {
        var shaderReflection = new ShaderReflection();

        foreach (var entryPoint in reflection.EntryPoints)
        {
            shaderReflection.EntryPoints.Add(new EntryPoint
            {
                Name = entryPoint.Name,
                Type = StageToType(entryPoint.Stage)
            });
        }

        foreach (var parameter in reflection.Parameters)
        {
            var binding = parameter.Bindings.FirstOrDefault();

            if (binding == null)
                continue;

            var shaderParameter = new ShaderParameter
            {
                Name = parameter.Name,
                Index = (int)binding.Index,
                Var = CreateShaderVar(parameter)
            };
            // TODO Validate?
            shaderParameter.Size = shaderParameter.Var.Layout.Size;

            shaderReflection.Parameters.Add(shaderParameter);
        }

        shaderReflection.RawReflectionJson = reflection.Json;

        return JsonSerializer.Serialize(shaderReflection);
    }

    private static ShaderVar CreateShaderVar(SlangParameter parameter)
    {
        var type = parameter.Type;

        var shaderVar = new ShaderVar
        {
            Layout = new PropertyLayout
            {
                Name = parameter.Name
            },
            Fields = new List<PropertyLayout>()
        };

        // ConstantBuffer -> ElementVarLayout
        var elementVarLayout = type.ConstantBuffer?.ElementVarLayout;

        if (elementVarLayout != null)
        {
            shaderVar.Layout = CreatePropertyLayout(
                elementVarLayout,
                parameter.Name);

            var fields = elementVarLayout.Type.Struct?.Fields;

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    if (field.Binding == null)
                        continue;

                    shaderVar.Fields.Add(
                        CreatePropertyLayout(
                            field,
                            field.Name));
                }
            }
        }
        else
        {
            // Non-constant-buffer parameter.
            // Use the parameter's own binding information.
            var binding = parameter.Bindings.FirstOrDefault();

            if (binding != null)
            {
                shaderVar.Layout = new PropertyLayout
                {
                    Name = parameter.Name,
                    Offset = (int)binding.Offset,
                    Size = (int)binding.Size
                };
            }
        }

        return shaderVar;
    }

    private static PropertyLayout CreatePropertyLayout(
        SlangVar variable,
        string name)
    {
        var binding = variable.Binding;

        return new PropertyLayout
        {
            Name = name,
            Offset = binding != null
                ? (int)binding.Offset
                : 0,
            Size = binding != null
                ? (int)binding.Size
                : 0
        };
    }

    private static ShaderType StageToType(SlangStage stage)
    {
        return stage switch
        {
            SlangStage.Vertex => ShaderType.Vertex,
            SlangStage.Fragment => ShaderType.Fragment,
            SlangStage.Compute => ShaderType.Compute,
            _ => throw new ArgumentException(
                $"Unsupported shader stage: {stage}")
        };
    }
}

public enum CompilationTarget
{
    SpirV,
    GlslEs3
}
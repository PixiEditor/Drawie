using System.Text;
using System.Text.Json;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Compilation;
using Slangc.NET;

namespace Drawie.ShaderCompiler.Compilation;

public class ShaderCompiler
{
    public string ModulesPath { get; set; }
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
                "-target", "spirv",
                "-profile", "spirv_1_3",
                "-I", ModulesPath
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
                Type = StageToType(entryPoint.Stage),
                Params = entryPoint.Parameters.Select(CreateShaderVar).ToArray()
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
            Name = parameter.Name,
            SemanticName = parameter.SemanticName,
            Layout = new PropertyLayout
            {
                Name = parameter.Name
            },
            Fields = new List<PropertyLayout>(),
            Type = (ShaderVarType)type.Kind,
            HasBindings = parameter.Bindings.Length > 0,
            ResourceType = type.Resource != null ? (ShaderVarShape)type.Resource.BaseShape : null
        };

        var elementVarLayout = type.ConstantBuffer?.ElementVarLayout;

        if (elementVarLayout != null)
        {
            shaderVar.Layout = CreatePropertyLayout(
                elementVarLayout,
                parameter.Name);
            
            var fields = elementVarLayout.Type.Struct?.Fields;

            if (fields != null)
            {
                shaderVar.Fields.AddRange(CreateFields(fields));
            }
        }
        else if (type.Struct != null)
        {
            shaderVar.Fields.AddRange(CreateFields(type.Struct.Fields));
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

    private static PropertyLayout[] CreateFields(SlangVar[] fields)
    {
        PropertyLayout[] layouts = new PropertyLayout[fields.Length];
        int offset = 0;
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];

            layouts[i] = CreatePropertyLayout(field, field.Name);
            layouts[i].Offset = offset;
            offset += layouts[i].Size;
        }

        return layouts;
    }

    private static PropertyLayout CreatePropertyLayout(
        SlangVar variable,
        string name)
    {
        int size = GetSize(variable);
        int offset = GetOffset(variable);

        return new PropertyLayout
        {
            Name = name,
            Offset = offset,
            Size = size,
            ScalarType = (ScalarType?)(variable.Type.Vector?.ElementType.Scalar?.ScalarType ?? variable.Type.Scalar?.ScalarType),
            ScalarsCount = (int)(variable.Type.Vector?.ElementCount ?? 1)
        };
    }

    private static int GetOffset(SlangVar variable)
    {
        if (variable.Binding != null) return (int)variable.Binding.Offset;

        return 0;
    }
    
    private static int GetSize(SlangVar variable)
    {
        if (variable.Type.Vector != null)
        {
            return GetScalarSize(variable.Type.Vector.ElementType.Scalar!.ScalarType) * (int)variable.Type.Vector.ElementCount;
        }
        if (variable.Binding != null) return (int)variable.Binding.Size;

        if (variable.Type.Kind == SlangTypeKind.Scalar)
        {
            var scalar = variable.Type.Scalar!.ScalarType;
            return GetScalarSize(scalar);
        }

        return 0;
    }

    private static int GetScalarSize(SlangScalarType scalar)
    {
        switch (scalar)
        {
            case SlangScalarType.Bool:
                return 4;
            case SlangScalarType.Int8:
                return 1;
            case SlangScalarType.UInt8:
                return 1;
            case SlangScalarType.Int16:
                return 2;
            case SlangScalarType.UInt16:
                return 2;
            case SlangScalarType.Int32:
                return 4;
            case SlangScalarType.UInt32:
                 return 4;
            case SlangScalarType.Int64:
                return 8;
            case SlangScalarType.UInt64:
                return 8;
            case SlangScalarType.Float16:
                return 2;
            case SlangScalarType.Float32:
                return 4;
            case SlangScalarType.Float64:
                return 8;
            case SlangScalarType.Unknown:
            case SlangScalarType.Void:
            default:
                return 0;
        }
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

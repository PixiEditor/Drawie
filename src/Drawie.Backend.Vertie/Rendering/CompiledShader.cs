using Drawie.RenderApi.Abstraction.Shaders;
using Slangc.NET;

namespace Drawie.Backend.Vertie.Rendering;

public class CompiledShader(byte[] shaderBytes, SlangReflection reflection)
{
    public byte[] ShaderBytes { get; } = shaderBytes;

    public ShaderType ShaderType { get; } = StageToType(reflection.EntryPoints.FirstOrDefault()?.Stage);
    public string EntryName { get; } = reflection.EntryPoints.FirstOrDefault()?.Name ?? throw new ArgumentException("Shader entry point not found in the compiled shader.");

    public SlangReflection Reflection { get; } = reflection;

    public IReadOnlyDictionary<string, UniformBlock> Properties => properties;

    private Dictionary<string, UniformBlock> properties = new Dictionary<string, UniformBlock>();

    public bool HasUniformBlock(string name)
    {
        return Reflection.Parameters.Any(x => x.Name == name);
    }

    public void SetUniformBlock(UniformBlock block)
    {
        properties[block.Name] = block;
        SetBlockLayout(block);
    }

    private void SetBlockLayout(UniformBlock block)
    {
        var elementVarLayout = Reflection.Parameters.FirstOrDefault(x => x.Name == block.Name)
            ?.Type.ConstantBuffer?.ElementVarLayout;

        if (elementVarLayout?.Binding == null)
        {
            throw new ArgumentException($"Uniform block binding '{block.Name}' not found in the compiled shader.");
        }

        List<UniformPropertyLayout> propLayouts = GetPropertyLayouts(elementVarLayout);
        
        block.ShaderLayout = new UniformBlockLayout()
        {
            Index = (int)elementVarLayout.Binding.Index,
            Size = (int)elementVarLayout.Binding.Size,
            UniformProperties = propLayouts
        };
    }
    
    private static ShaderType StageToType(SlangStage? stage)
    {
        if(stage == null) throw new ArgumentException("Shader stage cannot be null.");

        return stage switch
        {
            SlangStage.Vertex => ShaderType.Vertex,
            SlangStage.Fragment => ShaderType.Fragment,
            SlangStage.Compute => ShaderType.Compute,
            _ => throw new ArgumentException($"Unsupported shader stage: {stage}")
        };
    }

    private List<UniformPropertyLayout> GetPropertyLayouts(SlangVar elementVarLayout)
    {
        var fields = elementVarLayout.Type.Struct?.Fields;

        if (fields == null) return new List<UniformPropertyLayout>();

        List<UniformPropertyLayout> layouts = new List<UniformPropertyLayout>();
        foreach (var field in fields)
        {
            if(field.Binding == null) continue;
            layouts.Add(new UniformPropertyLayout(){UniformName = field.Name, Offset = (int)field.Binding.Offset, Size = (int)field.Binding.Size});
        }

        return layouts;
    }
}
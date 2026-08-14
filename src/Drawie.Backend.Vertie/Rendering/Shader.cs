using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.Backend.Vertie.Rendering;

public class Shader(byte[] shaderBytes, ShaderReflection reflection)
{
    public byte[] ShaderBytes { get; } = shaderBytes;

    public ShaderType ShaderType { get; } = reflection.EntryPoints.FirstOrDefault()?.Type ?? throw new ArgumentException("Shader type not found in the compiled shader.");
    public string EntryName { get; } = reflection.EntryPoints.FirstOrDefault()?.Name ?? throw new ArgumentException("Shader entry point not found in the compiled shader.");

    public ShaderReflection Reflection { get; } = reflection;

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
        var elementVarLayout = Reflection.Parameters.FirstOrDefault(x => x.Name == block.Name);

        if (elementVarLayout?.Var == null)
        {
            throw new ArgumentException($"Uniform block binding '{block.Name}' not found in the compiled shader.");
        }

        List<PropertyLayout> propLayouts = GetPropertyLayouts(elementVarLayout.Var);
        
        block.ShaderLayout = new UniformBlockLayout()
        {
            Index = elementVarLayout.Index,
            Size = elementVarLayout.Size,
            UniformProperties = propLayouts
        };
    }

    private List<PropertyLayout> GetPropertyLayouts(ShaderVar elementVar)
    {
        var fields = elementVar.Fields;

        if (fields == null) return new List<PropertyLayout>();

        return fields.ToList();
    }
}
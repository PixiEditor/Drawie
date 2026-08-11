using Drawie.RenderApi.Abstraction.Shaders;
using Slangc.NET;

namespace Drawie.Backend.Vertie.Rendering;

public class CompiledShader(byte[] vertex, byte[] fragment, SlangReflection vertexReflection, SlangReflection fragmentReflection)
{
    public byte[] Vertex { get; } = vertex;
    public byte[] Fragment { get; } = fragment;
    
    public SlangReflection VertexReflection { get; } = vertexReflection;
    public SlangReflection FragmentReflection { get; } = fragmentReflection;

    public IReadOnlyDictionary<string, UniformBlock> Properties => properties;

    private Dictionary<string, UniformBlock> properties = new Dictionary<string, UniformBlock>();

    public bool HasUniform(string name)
    {
        return Properties.ContainsKey(name);
    }

    public void SetUniformBlock(UniformBlock block)
    {
        properties[block.Name] = block;
        SetBlockLayout(block);
    }

    private void SetBlockLayout(UniformBlock block)
    {
        var elementVarLayout = VertexReflection.Parameters.FirstOrDefault(x => x.Name == block.Name)
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
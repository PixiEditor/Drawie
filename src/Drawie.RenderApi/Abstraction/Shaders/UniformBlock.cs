using System.Numerics;
using Drawie.Backend.Shaders.Common;

namespace Drawie.RenderApi.Abstraction.Shaders;

public class UniformBlock
{
    public string Name { get; set; }
    public List<ShaderProperty> Properties { get; set; } = new List<ShaderProperty>();
    public UniformBlockLayout ShaderLayout { get; set; }

    public UniformBlock AddProperty(ShaderProperty property)
    {
        Properties.Add(property);
        return this;
    }
    
    public UniformBlock(string name)
    {
        Name = name;
    }

    public void SetProperty(string name, object value)
    {
        var property = Properties.FirstOrDefault(p => p.UniformName == name);
        property?.ObjValue = value;
    }
}

public struct UniformBlockLayout
{
    public int Index { get; set; }
    public int Size { get; set; }
    public List<PropertyLayout> UniformProperties { get; set; }
}
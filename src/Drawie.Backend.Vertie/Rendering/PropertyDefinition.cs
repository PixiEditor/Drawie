using System.Numerics;
using System.Reflection.Metadata;
using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.Backend.Vertie.Rendering;

public class PropertyGroupDefinition
{
    public string Name { get; }
    public IReadOnlyList<PropertyDefinition> Properties => properties;
    public UniformBlockLayout ShaderLayout { get; set; }

    private List<PropertyDefinition> properties = new List<PropertyDefinition>();

    public PropertyGroupDefinition(string name)
    {
        Name = name;
        properties = new List<PropertyDefinition>();
    }

    public PropertyGroupDefinition AddProperty<T>(string name, T? defaultValue)
    {
        properties.Add(new PropertyDefinition(name, typeof(T), defaultValue));
        return this;
    }
}

public struct PropertyDefinition
{
    public string Name { get; }
    public Type ValueType { get; }
    public object? DefaultValue { get; }

    public PropertyDefinition(string name, Type valueType, object? defaultValue)
    {
        Name = name;
        ValueType = valueType;
        DefaultValue = defaultValue;
    }
}
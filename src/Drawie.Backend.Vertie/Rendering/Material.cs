using System.Numerics;
using Drawie.Backend.Vertie.Core;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Vertie.Rendering;

public class Material
{
    public string Name { get; set; }
    public Shader[] Shaders { get; set; }
    public Dictionary<string, PropertyGroupDefinition> Properties { get; set; } = new();
    public List<ITexture> Textures { get; set; } = new();

    private int _textureCount;

    public Material(string name, Shader[] shaders)
    {
        Name = name;
        Shaders = shaders;

        Properties.Add("Transform",
            new PropertyGroupDefinition("Transform")
                .AddProperty<Matrix4x4>("uModel", Matrix4x4.Identity)
                .AddProperty<Matrix4x4>("uView", Matrix4x4.Identity)
                .AddProperty<Matrix4x4>("uProjection", Matrix4x4.Identity));

        foreach (var propertyGroup in Properties)
        {
            foreach (var shader in shaders)
            {
                if (shader.HasUniformBlock(propertyGroup.Key))
                {
                    propertyGroup.Value.ShaderLayout = shader.GetLayoutFor(propertyGroup.Key);
                }
            }
        }
    }

    public void AddTexture(ITexture texture)
    {
        Textures.Add(texture);
    }

    /*
    public void UpdateShader()
    {
        foreach (var property in Properties)
        {
            ApplyToShader(property.Value);
        }
    }


    private void ApplyToShader(UniformBlock prop)
    {
        foreach (var shader in Shaders)
        {
            if (shader.HasUniformBlock(prop.Name))
            {
                shader.SetUniformBlock(prop);
            }
        }
    }*/
}
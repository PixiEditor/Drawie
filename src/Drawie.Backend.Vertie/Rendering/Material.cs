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
    public Dictionary<string, UniformBlock> Properties { get; set; } = new();
    public List<ITexture> Textures { get; set; } = new();

    private int _textureCount;

    public Material(string name, Shader[] shaders)
    {
        Name = name;
        Shaders = shaders;

        Properties.Add("Transform",
            new UniformBlock("Transform")
                .AddProperty(new ShaderProperty<Matrix4x4>("uModel", Matrix4x4.Identity))
                .AddProperty(new ShaderProperty<Matrix4x4>("uView", Matrix4x4.Identity))
                .AddProperty(new ShaderProperty<Matrix4x4>("uProjection", Matrix4x4.Identity)));
    }
    
    public void AddTexture(ITexture texture)
    {
        Textures.Add(texture);
    }

    public void Use(Camera camera)
    {
        Properties["Transform"].SetProperty("uView", camera.ViewMatrix);
        Properties["Transform"].SetProperty("uProjection", camera.ProjectionMatrix);
    }

    public void PrepareForObject(Transform transform)
    {
        Properties["Transform"].SetProperty("uModel", transform.ViewMatrix);
        UpdateShader();
    }

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
    }
}
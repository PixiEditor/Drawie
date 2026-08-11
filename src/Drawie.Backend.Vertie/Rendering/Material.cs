using System.Collections;
using System.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Vertie.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.Backend.Vertie.Rendering;

public class Material
{
    public string Name { get; set; }
    public CompiledShader Shader { get; set; }
    public Dictionary<string, UniformBlock> Properties { get; set; } = new();
    public TextureMaterial[] Textures { get; set; } = Array.Empty<TextureMaterial>();

    private int _textureCount;

    public Material(string name, CompiledShader shader)
    {
        Name = name;
        Shader = shader;

        Properties.Add("Transform",
            new UniformBlock("Transform")
                .AddProperty(new ShaderProperty<Matrix4x4>("uModel", Matrix4x4.Identity))
                .AddProperty(new ShaderProperty<Matrix4x4>("uView", Matrix4x4.Identity))
                .AddProperty(new ShaderProperty<Matrix4x4>("uProjection", Matrix4x4.Identity)));
        /*
        if (shader.HasUniform("viewPos"))
        {
            AddProperty<Vector3>("Camera", "viewPos");
        }
    */
    }

    public void Use(Camera camera)
    {
        Properties["Transform"].SetProperty("uView", camera.ViewMatrix);
        Properties["Transform"].SetProperty("uProjection", camera.ProjectionMatrix);
        /*
        if (Shader.HasUniform("viewPos"))
        {
            SetProperty("Camera", "viewPos", camera.Position);
        }
    */
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
        Shader.SetUniformBlock(prop);
    }
}

public struct TextureMaterial
{
    public string Name { get; set; }
    public ITexture Texture { get; set; }
}
using System.Collections;
using System.Numerics;
using Drawie.RenderApi;
using SilkNet.Rendering;

namespace Drawie.Backend.Vertie.Rendering;

public class Material
{
    public string Name { get; set; }
    public CompiledShader Shader { get; set; }
    public List<ShaderProperty> Properties { get; set; } = new List<ShaderProperty>();
    public TextureMaterial[] Textures { get; set; } = Array.Empty<TextureMaterial>();

    private int _textureCount;
    
    public Material(string name, CompiledShader shader)
    {
        Name = name;
        Shader = shader;
        
        AddProperty<Matrix4x4>("uModel");
        AddProperty<Matrix4x4>("uView");
        AddProperty<Matrix4x4>("uProjection");
        /*
        if (shader.HasUniform("viewPos"))
        {
            AddProperty<Vector3>("viewPos");
        }
    */
    }

    /*
    public void PrepareForObject(Transform transform)
    {
        SetProperty("uModel", transform.ViewMatrix);
        UpdateShader();
    }
    */


    public void AddProperty<T>(string name, T defaultValue = default) where T: struct
    {
        ShaderProperty<T> property = new ShaderProperty<T>(name, defaultValue);
        Properties.Add(property);
    }
    
    /*public void UpdateShader()
    {
        foreach (ShaderProperty property in Properties)
        {
            ApplyToShader(property);
        }
    }*/
    
    public void SetProperty<T>(string name, T value) where T : struct
    {
        foreach (var property in Properties)
        {
            if (property.UniformName == name)
            {
                if (property is ShaderProperty<T> prop)
                {
                    prop.Value = value;
                    return;
                }
                
                throw new Exception($"Property {name} is not of type {typeof(T)}");
            }
        }
        
        throw new Exception($"Property {name} does not exist");
    }

    /*private void ApplyToShader(ShaderProperty prop)
    {
        switch (prop.ObjValue)
        {
            case float floatValue:
                Shader.SetUniform(prop.UniformName, floatValue);
                break;
            case int intValue:
                Shader.SetUniform(prop.UniformName, intValue);
                break;
            case Vector3 vec3:
                Shader.SetUniform(prop.UniformName, vec3);
                break;
            case Matrix4x4 mat4:
                Shader.SetUniform(prop.UniformName, mat4);
                break;
            default:
                throw new Exception($"Property {prop.UniformName} is not a supported type");
        }
    }*/
}

public struct TextureMaterial
{
    public string Name { get; set; }
    public ITexture Texture { get; set; }
}
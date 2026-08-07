using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkNet.Rendering;

public class Material : IDisposable
{
    public string Name { get; set; }
    public Shader Shader { get; set; }
    public List<ShaderProperty> Properties { get; set; } = new List<ShaderProperty>();
    public Texture[] Textures { get; set; } = new Texture[32];
    
    private int _textureCount;
    
    public Material(string name, Shader shader)
    {
        Name = name;
        Shader = shader;
        
        AddProperty<Matrix4x4>("uModel");
        AddProperty<Matrix4x4>("uView");
        AddProperty<Matrix4x4>("uProjection");
        if (shader.HasUniform("viewPos"))
        {
            AddProperty<Vector3>("viewPos");
        }
    }

    public void Use(Camera camera)
    {
        BindTextures();
        Shader.Use();
        
        SetProperty("uView", camera.ViewMatrix);
        SetProperty("uProjection", camera.ProjectionMatrix);
        if (Shader.HasUniform("viewPos"))
        {
            SetProperty("viewPos", camera.Position);
        }
    }

    public void PrepareForObject(Transform transform)
    {
        SetProperty("uModel", transform.ViewMatrix);
        UpdateShader();
    }

    private void BindTextures()
    {
        for (var i = 0; i < _textureCount; i++)
        {
            TextureUnit unit = (TextureUnit)(0x84C0 + i);
            Textures[i].Bind(unit);
        }
    }

    public void AddTexture(Texture texture)
    {
        Textures[_textureCount] = texture;
        _textureCount++;
    }

    public void AddProperty<T>(string name, T defaultValue = default) where T: struct
    {
        ShaderProperty<T> property = new ShaderProperty<T>(name, defaultValue);
        Properties.Add(property);
    }
    
    public void UpdateShader()
    {
        foreach (ShaderProperty property in Properties)
        {
            ApplyToShader(property);
        }
    }
    
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

    private void ApplyToShader(ShaderProperty prop)
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
    }
    
    public void Dispose()
    {
        Shader.Dispose();
        foreach (var texture in Textures)
        {
            texture?.Dispose();
        }
    }
}
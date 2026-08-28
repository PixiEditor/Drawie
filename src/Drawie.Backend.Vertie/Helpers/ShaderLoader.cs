using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Rendering;

namespace Drawie.Backend.Vertie.Helpers;

public static class ShaderLoader
{
    public static Shader? LoadShader(string name)
    {
        using var shaderStream = ReadFromAssemblyStream(name + ".shader");
        using var reflectionStream = ReadFromAssemblyStream(name + ".reflection.json");

        using var memoryStream = new MemoryStream();
        shaderStream.CopyTo(memoryStream);
        byte[] shaderBytes = memoryStream.ToArray();
        
        using StreamReader reader = new StreamReader(reflectionStream);
        string reflectionJson = reader.ReadToEnd();

        if (string.IsNullOrEmpty(reflectionJson)) return null;

        ShaderReflection? reflection = JsonSerializer.Deserialize<ShaderReflection>(reflectionJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new ShaderReflectionContext()
        });
        
        return new Shader(shaderBytes, reflection);
    }

    private static Stream ReadFromAssemblyStream(string name)
    {
        Stream? stream = null;
        try
        {
            stream = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream("Drawie.Backend.Vertie.BuiltInShaders." + name)
                               ?? throw new InvalidOperationException("Shader not found");
            return stream;
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }
}
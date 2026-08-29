using System.Reflection;
using System.Text.Json;

namespace Drawie.Backend.Shaders.Common;

public static class ShaderLoader
{
    public static Shader? LoadShader(string name)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        using var shaderStream = ReadFromAssemblyStream(assembly, name + ".shader");
        using var reflectionStream = ReadFromAssemblyStream(assembly, name + ".reflection.json");


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

    private static Stream ReadFromAssemblyStream(Assembly assembly, string name)
    {
        Stream? stream = null;
        try
        {
            var assemblyName = assembly.GetName().Name;
            stream = assembly.GetManifestResourceStream($"{assemblyName}.BuiltInShaders." + name)
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
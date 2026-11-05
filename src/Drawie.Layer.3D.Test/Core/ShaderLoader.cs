namespace SilkNet;

public static class ShaderLoader
{
    public static string VertexShader;
    public static string UnlitShader;
    public static string LitShader;
    public static string BasicVertexShader;

    public static string LoadRaw(string name)
    {
        string filePath = Path.Join("Shaders", $"{name}.glsl");
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }

        throw new FileNotFoundException($"File {filePath} not found.");
    }
}
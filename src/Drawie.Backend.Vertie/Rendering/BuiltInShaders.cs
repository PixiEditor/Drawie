using Drawie.Backend.Vertie.Helpers;

namespace Drawie.Backend.Vertie.Rendering;

public static class BuiltInShaders
{
    static BuiltInShaders()
    {
        BasicVertexShader = ShaderLoader.LoadShader("BasicVertex");
        UnlitFragmentShader = ShaderLoader.LoadShader("Unlit");
    }
    
    public static Shader BasicVertexShader { get; private set; }
    public static Shader UnlitFragmentShader { get; private set; }
}
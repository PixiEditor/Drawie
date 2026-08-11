using Drawie.RenderApi.Abstraction.Shaders;

namespace Drawie.Backend.Vertie.Rendering;

public static class BuiltInShaders
{
    static BuiltInShaders()
    {
        var matShader = new ShaderDefinition(File.ReadAllText("Shaders/BasicVertex.slang"));
        var compiled = matShader.Compile();
        BasicVertexShader = compiled;
        
        var fragmentShader = new ShaderDefinition(File.ReadAllText("Shaders/Unlit.slang"));
        var compiledFragment = fragmentShader.Compile();
        UnlitFragmentShader = compiledFragment;
    }
    
    public static CompiledShader BasicVertexShader { get; private set; }
    public static CompiledShader UnlitFragmentShader { get; private set; }
}
namespace Drawie.Backend.Shaders.Common;

[Serializable]
public class ShaderReflection
{
    public List<ShaderParameter> Parameters { get; set; } = new List<ShaderParameter>();
    public List<EntryPoint> EntryPoints { get; set; } = new List<EntryPoint>();
    
    public string RawReflectionJson { get; set; }
}

[Serializable]
public class EntryPoint
{
    public string Name { get; set; }
    public ShaderType Type { get; set; }
}

[Serializable]
public class ShaderParameter
{
    public string Name { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public ShaderVar Var { get; set; }
}

[Serializable]
public class ShaderVar
{
    public PropertyLayout Layout { get; set; }
    public List<PropertyLayout> Fields { get; set; }
}
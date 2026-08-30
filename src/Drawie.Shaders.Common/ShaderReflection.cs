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
    public ShaderVar[] Params { get; set; }
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
    public string Name { get; set; }
    public string? SemanticName { get; set; }
    public PropertyLayout Layout { get; set; }
    public List<PropertyLayout>? Fields { get; set; }
    public ShaderVarType Type { get; set; }
    public ShaderVarShape? ResourceType { get; set; }
    public bool HasBindings { get; set; }
}

public enum ShaderVarType
{
    None = 0,
    Unknown = 0,
    Struct = 1,
    Array = 2,
    Matrix = 3,
    Vector = 4,
    Scalar = 5,
    ConstantBuffer = 6,
    Resource = 7,
    SamplerState = 8,
    TextureBuffer = 9,
    ShaderStorageBuffer = 10, // 0x0000000A
    ParameterBlock = 11, // 0x0000000B
    GenericTypeParameter = 12, // 0x0000000C
    Interface = 13, // 0x0000000D
    Feedback = 14, // 0x0000000E
    Pointer = 15, // 0x0000000F
    DynamicResource = 16, // 0x00000010
    OutputStream = 17, // 0x00000011
    MeshOutput = 18, // 0x00000012
    Specialized = 19, // 0x00000013
}

public enum ShaderVarShape
{
    Unknown,
    Texture1D,
    Texture2D,
    Texture3D,
    TextureCube,
    TextureBuffer,
    StructuredBuffer,
    ByteAddressBuffer,
    AccelerationStructure,
}
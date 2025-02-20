namespace Drawie.Backend.Core.Shaders;

public struct UniformDeclaration
{
    public UniformValueType DataType { get; }
    public string Name { get; }

    public UniformDeclaration(string name, UniformValueType dataType)
    {
        Name = name;
        DataType = dataType;
    }
}

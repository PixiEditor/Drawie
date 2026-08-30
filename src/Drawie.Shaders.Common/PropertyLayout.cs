namespace Drawie.Backend.Shaders.Common;

public struct PropertyLayout
{
    public string Name { get; set; }
    public int Offset { get; set; }
    public int Size { get; set; }
    public ScalarType? ScalarType { get; set; }
    public int ScalarsCount { get; set; }
}

public enum ScalarType
{
    Unknown,
    Void,
    Bool,
    Int8,
    UInt8,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float16,
    Float32,
    Float64,
}
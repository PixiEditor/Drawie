namespace Drawie.RenderApi.Abstraction.Shaders;

public class ShaderProperty
{
    public string UniformName { get; set; }
    public object? ObjValue { get; set; }
    public Type Type { get; set; }
    
    public ShaderProperty(string name)
    {
        UniformName = name;
    }
}

public class ShaderProperty<T> : ShaderProperty where T : unmanaged
{
    public T Value
    {
        get => (T)ObjValue;
        set => ObjValue = value;
    } 
    
    public ShaderProperty(string uniformName, T value) : base(uniformName)
    {
        UniformName = uniformName;
        ObjValue = value;
        Type = typeof(T);
    }
}
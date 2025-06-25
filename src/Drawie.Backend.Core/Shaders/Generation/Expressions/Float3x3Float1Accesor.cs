namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Float3x3Float1Accesor : Float1
{
    public Float3x3Float1Accesor(Float3x3 accessTo, int row, int col) : base(string.IsNullOrEmpty(accessTo.VariableName)
        ? string.Empty
        : $"{accessTo.VariableName}[{row}][{col}]")
    {
        Accesses = accessTo;
    }

    public Float3x3 Accesses { get; }
}

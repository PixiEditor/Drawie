namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

/// <summary>
///     This is a shader type that represents a boolean value.
/// </summary>
/// <param name="name">Name of the variable in shader code</param>
/// <param name="constant">Constant value of the variable.</param>
public class Bool(string name) : ShaderExpressionVariable<bool>(name)
{
    public override string ConstantValueString =>
        ConstantValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public override Expression? OverrideExpression { get; set; }

    public static implicit operator Bool(bool value) => new("") { ConstantValue = value };

    public static explicit operator bool(Bool value) => value.ConstantValue;
}

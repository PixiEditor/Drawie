using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace Drawie.Backend.Core.Shaders.Generation;

public abstract class ShaderExpressionVariable(string name) : Expression
{
    public string VariableName { get; set; } = name;
    public abstract string ConstantValueString { get; }

    public override string ToString()
    {
        return VariableName;
    }

    public override string ExpressionValue => OverrideExpression?.ExpressionValue ?? VarOrConst();
    public abstract Expression? OverrideExpression { get; set; }

    public abstract void SetConstantValue(object? value, Func<object, Type, object> convertFunc);
    
    private string VarOrConst()
    {
        return string.IsNullOrEmpty(VariableName) ? ConstantValueString : VariableName;
    }

    public abstract object GetConstant();
}

public abstract class ShaderExpressionVariable<TConstant>(string name)
    : ShaderExpressionVariable(name)
{
    public TConstant? ConstantValue { get; set; }

    public override void SetConstantValue(object? value, Func<object, Type, object> convertFunc)
    {
        if (value is TConstant constantValue)
        {
            ConstantValue = constantValue;
        }
        else
        {
            try
            {
                constantValue = (TConstant)convertFunc(value, typeof(TConstant));
                ConstantValue = constantValue;
            }
            catch (InvalidCastException)
            {
                ConstantValue = default;
            }
        }
    }
    
    public override object GetConstant()
    {
        return OverrideExpression is ShaderExpressionVariable overrideExpression ? overrideExpression.GetConstant() : ConstantValue;
    }
}

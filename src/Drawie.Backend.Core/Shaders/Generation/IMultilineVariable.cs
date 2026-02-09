using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace Drawie.Backend.Core.Shaders.Generation;

public interface IMultiValueVariable
{
    public ShaderExpressionVariable GetValueAt(int index);
    public void OverrideExpressionAt(int index, Expression? expression);
    public int GetValuesCount();
    public Expression? GetWholeNestedExpression();
    public void OverrideConstantValueAt(int i, object constant);
}

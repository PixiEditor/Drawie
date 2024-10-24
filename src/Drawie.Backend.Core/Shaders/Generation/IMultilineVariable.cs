namespace Drawie.Backend.Core.Shaders.Generation;

public interface IMultiValueVariable
{
    public ShaderExpressionVariable GetValueAt(int index);
}

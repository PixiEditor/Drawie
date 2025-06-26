﻿using System.Text;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation;

public class ShaderBuilder
{
    public Uniforms Uniforms { get; } = new Uniforms();

    private StringBuilder _bodyBuilder = new StringBuilder();

    private List<ShaderExpressionVariable> _variables = new List<ShaderExpressionVariable>();

    private Dictionary<DrawingSurface, SurfaceSampler> _samplers = new Dictionary<DrawingSurface, SurfaceSampler>();

    public BuiltInFunctions Functions { get; } = new();


    public ShaderBuilder(VecI resolution)
    {
        AddUniform("iResolution", resolution);
    }

    public Shader BuildShader()
    {
        string generatedSksl = ToSkSl();
        return Shader.Create(generatedSksl, Uniforms);
    }

    public string ToSkSl()
    {
        StringBuilder sb = new StringBuilder();
        AppendUniforms(sb);

        sb.AppendLine(Functions.BuildFunctions());

        sb.AppendLine("half4 main(float2 coords)");
        sb.AppendLine("{");
        sb.AppendLine("coords = coords / iResolution;");
        sb.Append(_bodyBuilder);
        sb.AppendLine("}");

        return sb.ToString();
    }

    private void AppendUniforms(StringBuilder sb)
    {
        foreach (var uniform in Uniforms)
        {
            string layout = string.IsNullOrEmpty(uniform.Value.LayoutOf)
                ? string.Empty
                : $"layout({uniform.Value.LayoutOf}) ";
            sb.AppendLine($"{layout}uniform {uniform.Value.UniformName} {uniform.Value.Name};");
        }
    }

    public SurfaceSampler AddOrGetSurface(DrawingSurface surface, ColorSampleMode sampleMode)
    {
        if (_samplers.TryGetValue(surface, out var sampler) && sampler.SampleMode == sampleMode)
        {
            return sampler;
        }

        string name = $"texture_{GetUniqueNameNumber()}";
        using var snapshot = surface.Snapshot();
        Uniforms[name] = new Uniform(name,
            sampleMode == ColorSampleMode.ColorManaged ? snapshot.ToShader() : snapshot.ToRawShader());
        var newSampler = new SurfaceSampler(name, sampleMode);
        _samplers[surface] = newSampler;

        return newSampler;
    }

    public Half4 Sample(SurfaceSampler texName, Expression pos)
    {
        string resultName = $"color_{GetUniqueNameNumber()}";
        Half4 result = new Half4(resultName);
        _variables.Add(result);
        _bodyBuilder.AppendLine(
            $"half4 {resultName} = {texName.VariableName}.eval({pos.ExpressionValue} * iResolution);");
        return result;
    }

    public void ReturnVar(Half4 colorValue)
    {
        string alphaExpression = colorValue.A.ExpressionValue;
        _bodyBuilder.AppendLine(
            $"half4 premultiplied = half4({colorValue.R.ExpressionValue} * {alphaExpression}, {colorValue.G.ExpressionValue} * {alphaExpression}, {colorValue.B.ExpressionValue} * {alphaExpression}, {alphaExpression});");
        _bodyBuilder.AppendLine($"return premultiplied;");
    }

    public void ReturnConst(Half4 colorValue)
    {
        _bodyBuilder.AppendLine($"return {colorValue.ConstantValueString};");
    }

    public void AddUniform(string uniformName, Color color)
    {
        Uniforms[uniformName] = new Uniform(uniformName, color);
    }

    public void AddUniform(string coords, VecD constCoordsConstantValue)
    {
        Uniforms[coords] = new Uniform(coords, constCoordsConstantValue);
    }

    public void AddUniform(string uniformName, float floatValue)
    {
        Uniforms[uniformName] = new Uniform(uniformName, floatValue);
    }

    public void AddUniform(string uniformName, Matrix3X3 matrixValue)
    {
        Uniforms[uniformName] = new Uniform(uniformName, matrixValue);
    }

    public void Set<T>(T contextPosition, T coordinateValue) where T : ShaderExpressionVariable
    {
        if (contextPosition.VariableName == coordinateValue.VariableName)
        {
            return;
        }

        _bodyBuilder.AppendLine($"{contextPosition.VariableName} = {coordinateValue.VariableName};");
    }

    public void SetConstant<T>(T contextPosition, T constantValueVar) where T : ShaderExpressionVariable
    {
        _bodyBuilder.AppendLine($"{contextPosition.VariableName} = {constantValueVar.ConstantValueString};");
    }

    public Float2 ConstructFloat2(Expression x, Expression y)
    {
        string name = $"vec2_{GetUniqueNameNumber()}";
        Float2 result = new Float2(name);
        _variables.Add(result);

        string xExpression = x.ExpressionValue;
        string yExpression = y.ExpressionValue;

        _bodyBuilder.AppendLine($"float2 {name} = float2({xExpression}, {yExpression});");
        return result;
    }

    public Float3 ConstructFloat3(Expression x, Expression y, Expression z)
    {
        string name = $"vec3_{GetUniqueNameNumber()}";
        Float3 result = new Float3(name);
        _variables.Add(result);

        string xExpression = x.ExpressionValue;
        string yExpression = y.ExpressionValue;
        string zExpression = z.ExpressionValue;

        _bodyBuilder.AppendLine($"float3 {name} = float3({xExpression}, {yExpression}, {zExpression});");
        return result;
    }

    public Float1 ConstructFloat1(Expression assignment)
    {
        string name = $"float_{GetUniqueNameNumber()}";
        Float1 result = new Float1(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"float {name} = {assignment.ExpressionValue};");
        return result;
    }

    public Int2 ConstructInt2(Expression first, Expression second)
    {
        string name = $"int2_{GetUniqueNameNumber()}";
        Int2 result = new Int2(name);
        _variables.Add(result);

        string firstExpression = first.ExpressionValue;
        string secondExpression = second.ExpressionValue;

        _bodyBuilder.AppendLine($"int2 {name} = int2({firstExpression}, {secondExpression});");
        return result;
    }

    public Half4 ConstructHalf4(Expression r, Expression g, Expression b, Expression a)
    {
        string name = $"color_{GetUniqueNameNumber()}";
        Half4 result = new Half4(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"half4 {name} = {Half4.ConstructorText(r, g, b, a)};");
        return result;
    }


    public Float3x3 ConstructFloat3x3(Expression m00, Expression m01, Expression m02, Expression m10, Expression m11, Expression m12, Expression m20, Expression m21, Expression m22)
    {
        string name = $"mat3_{GetUniqueNameNumber()}";
        Float3x3 result = new Float3x3(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"float3x3 {name} = float3x3({m00.ExpressionValue}, {m01.ExpressionValue}, {m02.ExpressionValue}, {m10.ExpressionValue}, {m11.ExpressionValue}, {m12.ExpressionValue}, {m20.ExpressionValue}, {m21.ExpressionValue}, {m22.ExpressionValue});");
        return result;
    }


    public Half4 AssignNewHalf4(Expression assignment) => AssignNewHalf4($"color_{GetUniqueNameNumber()}", assignment);

    public Half4 AssignNewHalf4(string name, Expression assignment)
    {
        Half4 result = new Half4(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"half4 {name} = {assignment.ExpressionValue};");
        return result;
    }

    public Float3 AssignNewFloat3(Expression expression)
    {
        string name = $"vec3_{GetUniqueNameNumber()}";
        Float3 result = new Float3(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"float3 {name} = {expression.ExpressionValue};");
        return result;
    }

    public Float2 AssignNewFloat2(Expression expression)
    {
        string name = $"vec2_{GetUniqueNameNumber()}";
        Float2 result = new Float2(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"float2 {name} = {expression.ExpressionValue};");
        return result;
    }


    public Float3x3 AssignNewFloat3x3(Expression matrixExpression)
    {
        string name = $"mat3_{GetUniqueNameNumber()}";
        Float3x3 result = new Float3x3(name);
        _variables.Add(result);

        _bodyBuilder.AppendLine($"float3x3 {name} = {matrixExpression.ExpressionValue};");
        return result;
    }

    public void Dispose()
    {
        _bodyBuilder.Clear();
        _variables.Clear();
        _samplers.Clear();

        foreach (var uniform in Uniforms)
        {
            uniform.Value.Dispose();
        }
    }

    public string GetUniqueNameNumber()
    {
        return (_variables.Count + Uniforms.Count + 1).ToString();
    }

    public void AssignVariable<T>(T variable, Expression assignment) where T : ShaderExpressionVariable
    {
        if (variable.VariableName == assignment.ExpressionValue)
        {
            return;
        }

        if(string.IsNullOrEmpty(variable.VariableName))
        {
            return;
        }

        _bodyBuilder.AppendLine($"{variable.VariableName} = {assignment.ExpressionValue};");
    }
}

public enum ColorSampleMode
{
    ColorManaged,
    Raw
}

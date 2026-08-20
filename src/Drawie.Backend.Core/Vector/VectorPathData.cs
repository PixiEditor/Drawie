using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Vector;

public struct VectorPathData : IEquatable<VectorPathData>
{
    public readonly IReadOnlyList<(PathVerb verb, VecF[] points, float conicWeight)> Verbs => verbs;

    private IReadOnlyList<(PathVerb verb, VecF[] points, float conicWeight)> verbs;

    public PathFillType FillType { get; set; } = PathFillType.Winding;
    public bool IsEmpty => verbs.Count == 0;
    public RectD TightBounds => tightBounds ??= CalculateBounds();
    public RectD Bounds => bounds ??= CalculateBounds();
    public double Length => length ??= CalculateLength();

    private RectD? tightBounds;
    private RectD? bounds;
    private double? length;

    public VectorPathData()
    {
    }

    public VectorPathData(VectorPath path)
    {
        var list = new List<(PathVerb verb, VecF[] points, float conicWeight)>();
        foreach (var verb in path)
        {
            list.Add(new ValueTuple<PathVerb, VecF[], float>(verb.verb, verb.points, verb.conicWeight));
        }

        verbs = list.AsReadOnly();
    }

    public static VectorPathData? FromSvgPath(string valueValue)
    {
        using var vectorPath = VectorPath.FromSvgPath(valueValue);
        if (vectorPath == null) return null;

        return new VectorPathData(vectorPath);
    }

    public static VectorPathData FromPoints(VecD[] points, bool forceClose)
    {
        using var vectorPath = VectorPath.FromPoints(points, forceClose);
        return new VectorPathData(vectorPath);
    }

    public VectorPathData(IReadOnlyList<(PathVerb verb, VecF[] points, float conicWeight)> pathVerbs)
    {
        verbs = pathVerbs.ToList();
    }

    internal VectorPathData(List<(PathVerb verb, VecF[] points, float conicWeight)> path, PathFillType fillType)
    {
        verbs = path;
        FillType = fillType;
    }

    public readonly VectorPath ToPath()
    {
        VectorPath path = new VectorPath();
        foreach (var valueTuple in Verbs)
        {
            switch (valueTuple.verb)
            {
                case PathVerb.Move:
                    path.MoveTo(valueTuple.points[0]);
                    break;
                case PathVerb.Line:
                    path.LineTo(valueTuple.points[0]);
                    break;
                case PathVerb.Quad:
                    path.QuadTo(valueTuple.points[0], valueTuple.points[1]);
                    break;
                case PathVerb.Conic:
                    path.ConicTo(valueTuple.points[0], valueTuple.points[1], valueTuple.conicWeight);
                    break;
                case PathVerb.Cubic:
                    path.CubicTo(valueTuple.points[0], valueTuple.points[1], valueTuple.points[2]);
                    break;
                case PathVerb.Close:
                    path.Close();
                    break;
                case PathVerb.Done:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return path;
    }

    private RectD CalculateBounds()
    {
        using var editablePath = ToPath();
        tightBounds = editablePath.TightBounds;
        bounds = editablePath.Bounds;
        return tightBounds ?? RectD.Empty;
    }

    private double CalculateLength()
    {
        using var editablePath = ToPath();
        return editablePath.Length;
    }

    public bool Equals(VectorPathData other)
    {
        return verbs.SequenceEqual(other.verbs) && Nullable.Equals(tightBounds, other.tightBounds) &&
               FillType == other.FillType;
    }

    public override bool Equals(object? obj)
    {
        return obj is VectorPathData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(verbs, tightBounds, (int)FillType);
    }

    public VectorPathData Op(VectorPathData second, VectorPathOp op)
    {
        using var firstEditable = ToPath();
        using var secondEditable = second.ToPath();

        return new VectorPathData(firstEditable.Op(secondEditable, op));
    }

    public Vec4D GetPositionAndTangentAtDistance(float absoluteOffset, bool forceClose)
    {
        using var path = ToPath();
        return path.GetPositionAndTangentAtDistance(absoluteOffset, forceClose);
    }

    public Matrix3X3 GetMatrixAtDistance(float absoluteOffset, bool forceClose, PathMeasureMatrixMode measureMode)
    {
        using var path = ToPath();
        return path.GetMatrixAtDistance(absoluteOffset, forceClose, measureMode);
    }

    public VectorPathData Offset(VecD offset)
    {
        var offsetVerbs =
            new List<(PathVerb verb, VecF[] points, float conicWeight)>(verbs.Count);

        foreach (var (verb, points, conicWeight) in verbs)
        {
            var offsetPoints = new VecF[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                offsetPoints[i] = new VecF(
                    (float)(points[i].X + offset.X),
                    (float)(points[i].Y + offset.Y));
            }

            offsetVerbs.Add((verb, offsetPoints, conicWeight));
        }

        return new VectorPathData(offsetVerbs, FillType);
    }

    public VectorPathData Transform(Matrix3X3 transformation)
    {
        using var path = ToPath();
        path.Transform(transformation);
        return new VectorPathData(path);
    }

    public string ToSvgPathData()
    {
        using var vectorPath = ToPath();
        return vectorPath.ToSvgPathData();
    }
}

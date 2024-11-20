﻿using System.Collections.ObjectModel;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Vector;

/// <summary>An interface for native compound geometric path implementations.</summary>
/// <remarks>A path encapsulates compound (multiple contour) geometric paths consisting of straight line segments, quadratic curves, and cubic curves.</remarks>
public class VectorPath : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PathImplementation.GetNativePath(ObjectPointer);

    public PathFillType FillType
    {
        get => DrawingBackendApi.Current.PathImplementation.GetFillType(this);
        set => DrawingBackendApi.Current.PathImplementation.SetFillType(this, value);
    }

    public PathConvexity Convexity
    {
        get => DrawingBackendApi.Current.PathImplementation.GetConvexity(this);
    }

    /// <summary>Gets a value indicating whether the path is a single oval or circle.</summary>
    public bool IsOval => DrawingBackendApi.Current.PathImplementation.IsPathOval(this);

    /// <summary>Gets a value indicating whether the path is a single, round rectangle.</summary>
    public bool IsRoundRect => DrawingBackendApi.Current.PathImplementation.IsRoundRect(this);

    /// <summary>Gets a value indicating whether the path is a single, straight line.</summary>
    public bool IsLine => DrawingBackendApi.Current.PathImplementation.IsLine(this);

    /// <summary>Gets a value indicating whether the path is a single rectangle.</summary>
    public bool IsRect => DrawingBackendApi.Current.PathImplementation.IsRect(this);

    /// <summary>Gets a set of flags indicating if the path contains one or more segments of that type.</summary>
    public PathSegmentMask SegmentMasks => DrawingBackendApi.Current.PathImplementation.GetSegmentMasks(this);

    /// <summary>Gets the number of verbs in the path.</summary>
    public int VerbCount => DrawingBackendApi.Current.PathImplementation.GetVerbCount(this);

    /// <summary>Gets the number of points on the path.</summary>
    public int PointCount => DrawingBackendApi.Current.PathImplementation.GetPointCount(this);

    /// <summary>Gets the "tight" bounds of the path. the control points of curves are excluded.</summary>
    /// <value>The tight bounds of the path.</value>
    public RectD TightBounds => DrawingBackendApi.Current.PathImplementation.GetTightBounds(this);

    public bool IsEmpty => VerbCount == 0;
    public RectD Bounds => DrawingBackendApi.Current.PathImplementation.GetBounds(this);

    public bool IsDisposed { get; private set; }

    public VecF LastPoint
    {
        get => DrawingBackendApi.Current.PathImplementation.GetLastPoint(this);
    }

    public IReadOnlyList<VecF> Points
    {
        get => DrawingBackendApi.Current.PathImplementation.GetPoints(ObjectPointer);
    }

    public event Action<VectorPath>? Changed;

    public static VectorPath FromSvgPath(string svgPath)
    {
        return DrawingBackendApi.Current.PathImplementation.FromSvgPath(svgPath);
    }

    public VectorPath(IntPtr nativePointer) : base(nativePointer)
    {
    }

    public VectorPath() : base(DrawingBackendApi.Current.PathImplementation.Create())
    {
    }

    public VectorPath(VectorPath other) : base(DrawingBackendApi.Current.PathImplementation.Clone(other))
    {
    }

    /// <param name="matrix">The matrix to use for transformation.</param>
    /// <summary>Applies a transformation matrix to the all the elements in the path.</summary>
    public void Transform(Matrix3X3 matrix)
    {
        DrawingBackendApi.Current.PathImplementation.Transform(this, matrix);
        Changed?.Invoke(this);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.Dispose(this);
        IsDisposed = true;
    }

    public void Reset()
    {
        DrawingBackendApi.Current.PathImplementation.Reset(this);
        Changed?.Invoke(this);
    }

    public void MoveTo(VecF vecF)
    {
        DrawingBackendApi.Current.PathImplementation.MoveTo(this, vecF);
        Changed?.Invoke(this);
    }

    public void LineTo(VecF vecF)
    {
        DrawingBackendApi.Current.PathImplementation.LineTo(this, vecF);
        Changed?.Invoke(this);
    }

    public void QuadTo(VecF mid, VecF vecF)
    {
        DrawingBackendApi.Current.PathImplementation.QuadTo(this, mid, vecF);
        Changed?.Invoke(this);
    }

    public void CubicTo(VecF mid1, VecF mid2, VecF vecF)
    {
        DrawingBackendApi.Current.PathImplementation.CubicTo(this, mid1, mid2, vecF);
        Changed?.Invoke(this);
    }

    public void ArcTo(RectI oval, int startAngle, int sweepAngle, bool forceMoveTo)
    {
        DrawingBackendApi.Current.PathImplementation.ArcTo(this, oval, startAngle, sweepAngle, forceMoveTo);
        Changed?.Invoke(this);
    }

    public void AddOval(RectI borders)
    {
        DrawingBackendApi.Current.PathImplementation.AddOval(this, borders);
        Changed?.Invoke(this);
    }

    /// <summary>
    ///     Compute the result of a logical operation on two paths.
    /// </summary>
    /// <param name="other">Other path.</param>
    /// <param name="pathOp">Logical operand.</param>
    /// <returns>Returns the resulting path if the operation was successful, otherwise null.</returns>
    public VectorPath Op(VectorPath other, VectorPathOp pathOp)
    {
        return DrawingBackendApi.Current.PathImplementation.Op(this, other, pathOp);
        Changed?.Invoke(this);
    }

    /// <summary>
    ///     Closes current contour.
    /// </summary>
    public void Close()
    {
        DrawingBackendApi.Current.PathImplementation.Close(this);
        Changed?.Invoke(this);
    }

    public string ToSvgPathData()
    {
        return DrawingBackendApi.Current.PathImplementation.ToSvgPathData(this);
    }

    public void AddRect(RectI rect, PathDirection direction = PathDirection.Clockwise)
    {
        DrawingBackendApi.Current.PathImplementation.AddRect(this, rect, direction);
        Changed?.Invoke(this);
    }

    public void AddPath(VectorPath path, AddPathMode mode)
    {
        DrawingBackendApi.Current.PathImplementation.AddPath(this, path, mode);
        Changed?.Invoke(this);
    }

    public bool Contains(float x, float y)
    {
        return DrawingBackendApi.Current.PathImplementation.Contains(this, x, y);
    }

    public VectorPath Simplify()
    {
        return DrawingBackendApi.Current.PathImplementation.Simplify(this);
    }

    public PathIterator CreateIterator(bool forceClose)
    {
        return DrawingBackendApi.Current.PathImplementation.CreateIterator(ObjectPointer, forceClose);
    }
}

public enum PathDirection
{
    Clockwise,
    CounterClockwise
}

public enum AddPathMode
{
    Append,
    Extend
}
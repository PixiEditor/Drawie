using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaPathImplementation : SkObjectImplementation<SKPath>, IVectorPathImplementation
    {
        private Dictionary<IntPtr, SKPath.Iterator> managedIterators = new Dictionary<IntPtr, SKPath.Iterator>();

        public PathFillType GetFillType(VectorPath path)
        {
            return (PathFillType)ManagedInstances[path.ObjectPointer].FillType;
        }

        public void SetFillType(VectorPath path, PathFillType fillType)
        {
            ManagedInstances[path.ObjectPointer].FillType = (SKPathFillType)fillType;
        }

        public PathConvexity GetConvexity(VectorPath path)
        {
            return (PathConvexity)ManagedInstances[path.ObjectPointer].Convexity;
        }

        public void Dispose(VectorPath path)
        {
            if (path.IsDisposed) return;
            ManagedInstances[path.ObjectPointer].Dispose();
            ManagedInstances.TryRemove(path.ObjectPointer, out _);
        }

        public bool IsPathOval(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsOval;
        }

        public bool IsRoundRect(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsRoundRect;
        }

        public bool IsLine(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsLine;
        }

        public bool IsRect(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsRect;
        }

        public PathSegmentMask GetSegmentMasks(VectorPath path)
        {
            return (PathSegmentMask)ManagedInstances[path.ObjectPointer].SegmentMasks;
        }

        public int GetVerbCount(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].VerbCount;
        }

        public int GetPointCount(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].PointCount;
        }

        public IntPtr Create()
        {
            SKPath path = new SKPath();
            ManagedInstances[path.Handle] = path;
            return path.Handle;
        }

        public IntPtr Clone(VectorPath other)
        {
            SKPath path = new SKPath(ManagedInstances[other.ObjectPointer]);
            ManagedInstances[path.Handle] = path;
            return path.Handle;
        }

        public RectD GetTightBounds(VectorPath vectorPath)
        {
            SKRect rect = ManagedInstances[vectorPath.ObjectPointer].TightBounds;
            return new RectD(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void Transform(VectorPath vectorPath, Matrix3X3 matrix)
        {
            ManagedInstances[vectorPath.ObjectPointer].Transform(matrix.ToSkMatrix());
        }

        public RectD GetBounds(VectorPath vectorPath)
        {
            SKRect rect = ManagedInstances[vectorPath.ObjectPointer].Bounds;
            return RectD.FromSides(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        public void Reset(VectorPath vectorPath)
        {
            ManagedInstances[vectorPath.ObjectPointer].Reset();
        }

        public void AddRect(VectorPath path, RectI rect, PathDirection direction)
        {
            ManagedInstances[path.ObjectPointer].AddRect(rect.ToSkRect(), (SKPathDirection)direction);
        }

        public void MoveTo(VectorPath vectorPath, VecF vecF)
        {
            ManagedInstances[vectorPath.ObjectPointer].MoveTo(vecF.ToSkPoint());
        }

        public void LineTo(VectorPath vectorPath, VecF vecF)
        {
            ManagedInstances[vectorPath.ObjectPointer].LineTo(vecF.ToSkPoint());
        }

        public void QuadTo(VectorPath vectorPath, VecF mid, VecF vecF)
        {
            ManagedInstances[vectorPath.ObjectPointer].QuadTo(mid.ToSkPoint(), vecF.ToSkPoint());
        }

        public void CubicTo(VectorPath vectorPath, VecF mid1, VecF mid2, VecF vecF)
        {
            ManagedInstances[vectorPath.ObjectPointer].CubicTo(mid1.ToSkPoint(), mid2.ToSkPoint(), vecF.ToSkPoint());
        }

        public void ArcTo(VectorPath vectorPath, RectI oval, int startAngle, int sweepAngle, bool forceMoveTo)
        {
            ManagedInstances[vectorPath.ObjectPointer].ArcTo(oval.ToSkRect(), startAngle, sweepAngle, forceMoveTo);
        }

        public void AddOval(VectorPath vectorPath, RectI borders)
        {
            ManagedInstances[vectorPath.ObjectPointer].AddOval(borders.ToSkRect());
        }

        public void AddPath(VectorPath vectorPath, VectorPath other, AddPathMode mode)
        {
            ManagedInstances[vectorPath.ObjectPointer]
                .AddPath(ManagedInstances[other.ObjectPointer], (SKPathAddMode)mode);
        }

        public object GetNativePath(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }

        public VecF GetLastPoint(VectorPath vectorPath)
        {
            SKPoint point = ManagedInstances[vectorPath.ObjectPointer].LastPoint;
            return new VecF(point.X, point.Y);
        }

        public VectorPath FromSvgPath(string svgPath)
        {
            SKPath skPath = SKPath.ParseSvgPathData(svgPath);

            ManagedInstances[skPath.Handle] = skPath;
            return new VectorPath(skPath.Handle);
        }
        
        public VecF[] GetPoints(IntPtr objectPointer)
        {
            SKPoint[] points = ManagedInstances[objectPointer].Points;
            return CastUtility.UnsafeArrayCast<SKPoint, VecF>(points);
        }

        public PathIterator CreateIterator(IntPtr objectPointer, bool forceClose)
        {
            SKPath.Iterator iterator = ManagedInstances[objectPointer].CreateIterator(forceClose);
            managedIterators[objectPointer] = iterator;
            return new PathIterator(iterator.Handle);
        }

        public void DisposeIterator(IntPtr objectPointer)
        {
            managedIterators[objectPointer].Dispose();
            managedIterators.Remove(objectPointer);
        }

        public object GetNativeIterator(IntPtr objectPointer)
        {
            return managedIterators[objectPointer];
        }

        /// <summary>
        ///     Compute the result of a logical operation on two paths.
        /// </summary>
        /// <param name="vectorPath">Source operand</param>
        /// <param name="ellipsePath">The second operand.</param>
        /// <param name="pathOp">The logical operator.</param>
        /// <returns>Returns the resulting path if the operation was successful, otherwise null.h</returns>
        public VectorPath Op(VectorPath vectorPath, VectorPath ellipsePath, VectorPathOp pathOp)
        {
            SKPath skPath = ManagedInstances[vectorPath.ObjectPointer]
                .Op(ManagedInstances[ellipsePath.ObjectPointer], (SKPathOp)pathOp);
            ManagedInstances[skPath.Handle] = skPath;
            return new VectorPath(skPath.Handle);
        }

        public VectorPath Simplify(VectorPath path)
        {
            SKPath skPath = ManagedInstances[path.ObjectPointer].Simplify();
            ManagedInstances[skPath.Handle] = skPath;
            return new VectorPath(skPath.Handle);
        }

        public void Close(VectorPath vectorPath)
        {
            ManagedInstances[vectorPath.ObjectPointer].Close();
        }

        public string ToSvgPathData(VectorPath vectorPath)
        {
            return ManagedInstances[vectorPath.ObjectPointer].ToSvgPathData();
        }

        public bool Contains(VectorPath vectorPath, float x, float y)
        {
            return ManagedInstances[vectorPath.ObjectPointer].Contains(x, y);
        }
    }
}

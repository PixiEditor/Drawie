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

        private Dictionary<IntPtr, SKPath.RawIterator> managedRawIterators =
            new Dictionary<IntPtr, SKPath.RawIterator>();

        private SKPoint[] intermediatePoints = new SKPoint[4];

        public PathFillType GetFillType(VectorPath path)
        {
            return (PathFillType)this[path.ObjectPointer].FillType;
        }

        public void SetFillType(VectorPath path, PathFillType fillType)
        {
            this[path.ObjectPointer].FillType = (SKPathFillType)fillType;
        }

        public PathConvexity GetConvexity(VectorPath path)
        {
            return (PathConvexity)this[path.ObjectPointer].Convexity;
        }

        public void Dispose(VectorPath path)
        {
            if (path.IsDisposed) return;
            UnmanageAndDispose(path.ObjectPointer);
        }

        public bool IsPathOval(VectorPath path)
        {
            return this[path.ObjectPointer].IsOval;
        }

        public bool IsRoundRect(VectorPath path)
        {
            return this[path.ObjectPointer].IsRoundRect;
        }

        public bool IsLine(VectorPath path)
        {
            return this[path.ObjectPointer].IsLine;
        }

        public bool IsRect(VectorPath path)
        {
            return this[path.ObjectPointer].IsRect;
        }

        public PathSegmentMask GetSegmentMasks(VectorPath path)
        {
            return (PathSegmentMask)this[path.ObjectPointer].SegmentMasks;
        }

        public int GetVerbCount(VectorPath path)
        {
            return this[path.ObjectPointer].VerbCount;
        }

        public int GetPointCount(VectorPath path)
        {
            return this[path.ObjectPointer].PointCount;
        }

        public IntPtr Create()
        {
            SKPath path = new SKPath();
            AddManagedInstance(path);
            return path.Handle;
        }

        public IntPtr Clone(VectorPath other)
        {
            SKPath path = new SKPath(this[other.ObjectPointer]);
            AddManagedInstance(path);
            return path.Handle;
        }

        public RectD GetTightBounds(VectorPath vectorPath)
        {
            SKRect rect = this[vectorPath.ObjectPointer].TightBounds;
            return new RectD(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void Transform(VectorPath vectorPath, Matrix3X3 matrix)
        {
            SKMatrix mappedMatrix = matrix.ToSkMatrix();
            this[vectorPath.ObjectPointer].Transform(in mappedMatrix);
        }

        public RectD GetBounds(VectorPath vectorPath)
        {
            SKRect rect = this[vectorPath.ObjectPointer].Bounds;
            return RectD.FromSides(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        public void Reset(VectorPath vectorPath)
        {
            this[vectorPath.ObjectPointer].Reset();
        }

        public void AddRect(VectorPath path, RectD rect, PathDirection direction)
        {
            this[path.ObjectPointer].AddRect(rect.ToSkRect(), (SKPathDirection)direction);
        }

        public void AddRoundRect(VectorPath path, RectD rect, VecD cornerRadius, PathDirection direction)
        {
            this[path.ObjectPointer].AddRoundRect(rect.ToSkRect(), (float)cornerRadius.X, (float)cornerRadius.Y,
                (SKPathDirection)direction);
        }

        public void MoveTo(VectorPath vectorPath, VecF vecF)
        {
            this[vectorPath.ObjectPointer].MoveTo(vecF.ToSkPoint());
        }

        public void LineTo(VectorPath vectorPath, VecF vecF)
        {
            this[vectorPath.ObjectPointer].LineTo(vecF.ToSkPoint());
        }

        public void QuadTo(VectorPath vectorPath, VecF control, VecF point)
        {
            this[vectorPath.ObjectPointer].QuadTo(control.ToSkPoint(), point.ToSkPoint());
        }

        public void CubicTo(VectorPath vectorPath, VecF mid1, VecF mid2, VecF point)
        {
            this[vectorPath.ObjectPointer].CubicTo(mid1.ToSkPoint(), mid2.ToSkPoint(), point.ToSkPoint());
        }

        public void ArcTo(VectorPath vectorPath, RectD oval, int startAngle, int sweepAngle, bool forceMoveTo)
        {
            this[vectorPath.ObjectPointer].ArcTo(oval.ToSkRect(), startAngle, sweepAngle, forceMoveTo);
        }

        public void ConicTo(VectorPath vectorPath, VecF mid, VecF end, float weight)
        {
            this[vectorPath.ObjectPointer].ConicTo(mid.ToSkPoint(), end.ToSkPoint(), weight);
        }

        public void AddOval(VectorPath vectorPath, RectD borders)
        {
            this[vectorPath.ObjectPointer].AddOval(borders.ToSkRect());
        }

        public void AddPath(VectorPath vectorPath, VectorPath other, AddPathMode mode)
        {
            this[vectorPath.ObjectPointer]
                .AddPath(this[other.ObjectPointer], (SKPathAddMode)mode);
        }

        public void AddPath(VectorPath vectorPath, VectorPath other, Matrix3X3 matrixToOther, AddPathMode mode)
        {
            this[vectorPath.ObjectPointer]
                .AddPath(this[other.ObjectPointer], matrixToOther.ToSkMatrix(), (SKPathAddMode)mode);
        }

        public object GetNativePath(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public VecF GetLastPoint(VectorPath vectorPath)
        {
            SKPoint point = this[vectorPath.ObjectPointer].LastPoint;
            return new VecF(point.X, point.Y);
        }

        public VectorPath? FromSvgPath(string svgPath)
        {
            SKPath skPath = SKPath.ParseSvgPathData(svgPath);

            if (skPath == null)
            {
                return null;
            }

            AddManagedInstance(skPath);
            return new VectorPath(skPath.Handle);
        }

        public VecF[] GetPoints(IntPtr objectPointer)
        {
            SKPoint[] points = this[objectPointer].Points;
            return CastUtility.UnsafeArrayCast<SKPoint, VecF>(points);
        }

        public PathIterator CreateIterator(IntPtr objectPointer, bool forceClose)
        {
            SKPath.Iterator iterator = this[objectPointer].CreateIterator(forceClose);
            managedIterators[iterator.Handle] = iterator;
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

        public bool IsCloseContour(IntPtr objectPointer)
        {
            return managedIterators[objectPointer].IsCloseContour();
        }

        public float GetConicWeight(IntPtr objectPointer)
        {
            return managedIterators[objectPointer].ConicWeight();
        }

        public float GetRawConicWeight(IntPtr objectPointer)
        {
            return managedRawIterators[objectPointer].ConicWeight();
        }

        public Vec4D GetPositionAndTangentAtDistance(IntPtr objectPointer, float distance, bool forceClose)
        {
            using SKPathMeasure measure = new SKPathMeasure(this[objectPointer], forceClose);
            if (measure.GetPositionAndTangent(distance, out var pos, out var tangent))
            {
                return new Vec4D(pos.X, pos.Y, tangent.X, tangent.Y);
            }

            return Vec4D.Zero;
        }

        public Matrix3X3 GetMatrixAtDistance(IntPtr objectPointer, float distance, bool forceClose, PathMeasureMatrixMode mode)
        {
            using SKPathMeasure measure = new SKPathMeasure(this[objectPointer], forceClose);
            return measure.GetMatrix(distance, (SKPathMeasureMatrixFlags)(int)mode).ToMatrix3X3();
        }

        public double GetLength(IntPtr objectPointer, bool forceClose)
        {
            using SKPathMeasure measure = new SKPathMeasure(this[objectPointer], forceClose);
            return measure.Length;
        }

        public PathVerb IteratorNextVerb(IntPtr objectPointer, VecF[] points)
        {
            // TODO: maybe there is a way to unsafely cast the array directly
            ResetIntermediatePoints();
            var next = (PathVerb)managedIterators[objectPointer].Next(intermediatePoints);
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new VecF(intermediatePoints[i].X, intermediatePoints[i].Y);
            }

            return next;
        }

        public RawPathIterator CreateRawIterator(IntPtr objectPointer)
        {
            SKPath.RawIterator iterator = this[objectPointer].CreateRawIterator();
            managedRawIterators[iterator.Handle] = iterator;
            return new RawPathIterator(iterator.Handle);
        }

        public PathVerb RawIteratorNextVerb(IntPtr objectPointer, VecF[] points)
        {
            SKPath.RawIterator iterator = managedRawIterators[objectPointer];
            ResetIntermediatePoints();
            var next = (PathVerb)iterator.Next(intermediatePoints);
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new VecF(intermediatePoints[i].X, intermediatePoints[i].Y);
            }

            return next;
        }

        public void DisposeRawIterator(IntPtr objectPointer)
        {
            managedRawIterators[objectPointer].Dispose();
            managedRawIterators.Remove(objectPointer);
        }

        public object GetNativeRawIterator(IntPtr objectPointer)
        {
            return managedRawIterators[objectPointer];
        }

        public void Offset(VectorPath vectorPath, VecD delta)
        {
            this[vectorPath.ObjectPointer].Offset((float)delta.X, (float)delta.Y);
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
            SKPath skPath = this[vectorPath.ObjectPointer]
                .Op(this[ellipsePath.ObjectPointer], (SKPathOp)pathOp);

            if (skPath == null)
            {
                var emptyPath = new SKPath();
                AddManagedInstance(emptyPath);
                return new VectorPath(emptyPath.Handle);
            }

            AddManagedInstance(skPath);
            return new VectorPath(skPath.Handle);
        }

        public VectorPath Simplify(VectorPath path)
        {
            SKPath skPath = this[path.ObjectPointer].Simplify();
            AddManagedInstance(skPath);
            return new VectorPath(skPath.Handle);
        }

        public void Close(VectorPath vectorPath)
        {
            this[vectorPath.ObjectPointer].Close();
        }

        public string ToSvgPathData(VectorPath vectorPath)
        {
            return this[vectorPath.ObjectPointer].ToSvgPathData();
        }

        public bool Contains(VectorPath vectorPath, float x, float y)
        {
            return this[vectorPath.ObjectPointer].Contains(x, y);
        }

        private void ResetIntermediatePoints()
        {
            for (int i = 0; i < intermediatePoints.Length; i++)
            {
                intermediatePoints[i] = new SKPoint();
            }
        }
    }
}

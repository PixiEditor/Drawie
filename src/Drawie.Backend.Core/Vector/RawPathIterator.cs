using System.Collections;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Vector;

public class RawPathIterator : NativeObject, IEnumerator<(PathVerb verb, VecF[] points, float conicWeight)>
{
    public RawPathIterator(IntPtr objPtr) : base(objPtr)
    {
    }

    private PathVerb currentVerb;
    private float currentConicWeight;
    private VecF[] iteratorPoints;
    private bool wasDone;

    public override object Native => DrawingBackendApi.Current.PathImplementation.GetNativeRawIterator(ObjectPointer);

    public PathVerb Next(VecF[] points)
    {
        return DrawingBackendApi.Current.PathImplementation.RawIteratorNextVerb(ObjectPointer, points);
    }

    public float GetConicWeight()
    {
        return DrawingBackendApi.Current.PathImplementation.GetRawConicWeight(ObjectPointer);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.DisposeRawIterator(ObjectPointer);
    }

    bool IEnumerator.MoveNext()
    {
        if (wasDone) return false;

        iteratorPoints = new VecF[4];
        currentVerb = Next(iteratorPoints);
        currentConicWeight = GetConicWeight();
        bool done = currentVerb == PathVerb.Done;
        wasDone = done;
        return true;
    }

    void IEnumerator.Reset()
    {
        throw new ArgumentException("Path Iterators can't be reused");
    }

    (PathVerb verb, VecF[] points, float conicWeight) IEnumerator<(PathVerb verb, VecF[] points, float conicWeight)>.
        Current => (currentVerb, iteratorPoints, currentConicWeight);

    object? IEnumerator.Current => (currentVerb, iteratorPoints, currentConicWeight);
}

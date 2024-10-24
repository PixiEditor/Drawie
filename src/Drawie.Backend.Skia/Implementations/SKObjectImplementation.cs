using System.Collections.Concurrent;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public abstract class SkObjectImplementation<T> where T : SKObject
    {
        internal readonly ConcurrentDictionary<IntPtr, T> ManagedInstances = new ConcurrentDictionary<IntPtr, T>();
        
        public virtual void DisposeObject(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Dispose();
            ManagedInstances.TryRemove(objPtr, out _);
        }
        
        public T this[IntPtr objPtr]
        {
            get => ManagedInstances.TryGetValue(objPtr, out var instance) ? instance : throw new ObjectDisposedException(nameof(objPtr));
            set => ManagedInstances[objPtr] = value;
        }

        public void DisposeAll()
        {
            foreach (var instance in ManagedInstances.Values)
            {
                instance.Dispose();
            }
            
            ManagedInstances.Clear();
        }
    }
}

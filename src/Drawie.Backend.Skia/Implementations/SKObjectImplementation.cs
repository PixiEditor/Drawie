using System.Collections.Concurrent;
using System.Diagnostics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public abstract class SkObjectImplementation<T> where T : SKObject
    {
        public int Count => ManagedInstances.Count;
        private readonly ConcurrentDictionary<IntPtr, T> ManagedInstances = new ConcurrentDictionary<IntPtr, T>();
        private ulong handleCounter = 1;

#if DRAWIE_TRACE
        protected static Dictionary<T, string> sources = new();
#endif

        internal IntPtr AddManagedInstance(T instance)
        {
            IntPtr handle = GetNextHandle();
            if (ManagedInstances.TryAdd(handle, instance))
            {
#if DRAWIE_TRACE
                sources[instance] = Environment.StackTrace;
#endif
            }
            else
            {
                throw new InvalidOperationException(
                    $"Native handle {instance.Handle} is already registered. " +
                    $"Existing: {ManagedInstances[instance.Handle]}, " +
                    $"New: {instance}");
            }

            return handle;
        }

        protected IntPtr GetNextHandle()
        {
            return (IntPtr)Interlocked.Increment(ref handleCounter);
        }

        public bool TryGetInstance(IntPtr objPtr, out T? instance)
        {
            return ManagedInstances.TryGetValue(objPtr, out instance);
        }

        public void UnmanageAndDispose(IntPtr objPtr)
        {
            if (ManagedInstances.TryRemove(objPtr, out var instance))
            {
                if (instance == null) return;
#if DRAWIE_TRACE
                sources.Remove(instance);
#endif
                instance.Dispose();
            }
        }

        public void UpdateManagedInstance(IntPtr objPtr, T instance)
        {
            if (ManagedInstances.TryRemove(objPtr, out var managedInstance))
            {
                if (managedInstance == null) return;
#if DRAWIE_TRACE
                Untrace(managedInstance);
#endif
                managedInstance.Dispose();
            }

            if (ManagedInstances.TryAdd(objPtr, instance))
            {
#if DRAWIE_TRACE
                Trace(instance);
#endif
            }
        }

        public T? GetInstanceOrDefault(IntPtr obj)
        {
            return ManagedInstances.GetValueOrDefault(obj);
        }

        public void Unmanage(IntPtr objPtr)
        {
            if (ManagedInstances.TryRemove(objPtr, out var instance))
            {
#if DRAWIE_TRACE
                sources.Remove(instance);
#endif
            }
        }

        public T this[IntPtr objPtr]
        {
            get => ManagedInstances.TryGetValue(objPtr, out var instance)
                ? instance
                : throw new ObjectDisposedException(nameof(objPtr));
        }

        public void DisposeAll()
        {
            foreach (var instance in ManagedInstances.Values)
            {
                instance.Dispose();
            }

            ManagedInstances.Clear();

#if DRAWIE_TRACE
            sources.Clear();
#endif
        }

#if DRAWIE_TRACE
        public static Dictionary<string, int> GetFlattenedSources()
        {
            Dictionary<string, int> flattenedSources = new();
            foreach (var source in sources)
            {
                string stackTrace = source.Value;
                if (!flattenedSources.TryAdd(stackTrace, 1))
                {
                    flattenedSources[stackTrace]++;
                }
            }

            return flattenedSources;
        }

        protected static void Untrace(T shader)
        {
            sources.Remove(shader);
        }

        protected static void Trace(T shader)
        {
            sources[shader] = Environment.StackTrace;
        }

#endif
        public IntPtr? FindManagedInstanceHandle(T native)
        {
            foreach (var kvp in ManagedInstances)
            {
                if (EqualityComparer<T>.Default.Equals(kvp.Value, native))
                {
                    return kvp.Key;
                }
            }
            return null;
        }
    }
}

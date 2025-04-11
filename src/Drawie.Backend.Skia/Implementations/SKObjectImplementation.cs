using System.Collections.Concurrent;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public abstract class SkObjectImplementation<T> where T : SKObject
    {
        public int Count => ManagedInstances.Count;
        private readonly ConcurrentDictionary<IntPtr, T> ManagedInstances = new ConcurrentDictionary<IntPtr, T>();

#if DRAWIE_TRACE
        protected static Dictionary<T, string> sources = new();
#endif

        internal void AddManagedInstance(T instance)
        {
            if (ManagedInstances.TryAdd(instance.Handle, instance))
            {
#if DRAWIE_TRACE
                sources[instance] = Environment.StackTrace;
#endif
            }
        }

        internal void AddManagedInstance(IntPtr handle, T instance)
        {
            if (ManagedInstances.TryAdd(handle, instance))
            {
#if DRAWIE_TRACE
                        sources[instance] = Environment.StackTrace;
#endif
            }
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

        public void UnmanageAndDispose(T instance)
        {
            if (ManagedInstances.TryRemove(instance.Handle, out var managedInstance))
            {
                if (managedInstance == null) return;

#if DRAWIE_TRACE
                Untrace(managedInstance);
#endif
            }

            instance.Dispose();
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
    }
}

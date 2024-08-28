using System.Runtime.InteropServices;

namespace Drawie.Skia
{
    public static class CastUtility
    {
        public static T2[] UnsafeArrayCast<T1, T2>(T1[] source) where T1 : struct where T2 : struct
        {
            return MemoryMarshal.Cast<T1, T2>(source).ToArray();
        }
    }
}

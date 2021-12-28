using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public class DynamicElement<T>
    {
        public T[] Value;
        public GCHandle ValueHandle;
        //public Memory<T> Memory;
    }
}
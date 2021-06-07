using System.Collections.Generic;

namespace Droid.Core
{
    public static class Extensions
    {
        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
        }

        public static void SetNum<T>(this List<T> source, int newNum, bool resize = true)
        {
        }

        public static void Resize(this List<T> source, int newSize)
        {
        }

        public static void AssureSize(this List<T> source, int newSize)
        {
        }
    }
}
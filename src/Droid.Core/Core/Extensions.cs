using System;
using System.Collections.Generic;
using System.Reflection;

namespace Droid.Core
{
    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
        }

        public static void SetNum<T>(this List<T> source, int newNum, bool resize = true)
        {
        }

        public static void Resize<T>(this List<T> source, int newSize)
        {
        }

        public static void AssureSize<T>(this List<T> source, int newSize)
        {
        }

        public static T[] Ptr<T>(this List<T> source)
            => (T[])ItemsField.GetValue(source);

        public static Span<T> Ptr<T>(this List<T> source, int startIndex)
            => ((T[])ItemsField.GetValue(source)).AsSpan(startIndex);
    }
}
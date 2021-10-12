using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;

namespace System.NumericsX
{
    public static class intX
    {
        public const int ALLOC16 = 4;

        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

        public static int Parse(string s)
            => int.TryParse(s, out var z) ? z : 0;
    }

    public static class floatX
    {
        public const int ALLOC16 = 4;

        public static float Parse(string s)
            => float.TryParse(s, out var z) ? z : 0f;
    }

    public static class boolX
    {
        public const int ALLOC16 = 15;
    }

    public static class byteX
    {
        public const int ALLOC16 = 15;

        public static int MD5Checksum(byte[] buffer)
        {
            using var md5 = MD5.Create();
            var digest = md5.ComputeHash(buffer);
            return digest[0] ^ digest[1] ^ digest[2] ^ digest[3];
        }
    }

    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        #region List

        public static ref T Ref<T>(this List<T> source, int index)
            => ref ((T[])ItemsField.GetValue(source))[index];

        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
        }

        public static int AddUnique<T>(this List<T> source, T item)
        {
            var index = source.FindIndex(x => x.Equals(item));
            if (index < 0) index = source.Add_(item);
            return index;
        }

        public static T[] SetNum<T>(this List<T> source, int newNum, bool resize = true)
        {
            source.Capacity = newNum;
            return (T[])ItemsField.GetValue(source);
        }

        public static void SetGranularity<T>(this List<T> source, int granularity)
        {
        }

        public static void Resize<T>(this List<T> source, int newSize)
        {
        }

        public static void Resize<T>(this List<T> source, int newSize, int newGranularity)
        {
        }

        public static void AssureSize<T>(this List<T> source, int newSize)
        {
        }

        public static T[] Ptr<T>(this List<T> source)
            => (T[])ItemsField.GetValue(source);

        public static Span<T> Ptr<T>(this List<T> source, int startIndex)
            => ((T[])ItemsField.GetValue(source)).AsSpan(startIndex);

        #endregion
    }
}
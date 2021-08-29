using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace System.NumericsX
{
    public static class intX
    {
        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

        public static int Parse(string s)
            => int.Parse(s);
    }

    public static class floatX
    {
        public static float Parse(string s)
            => float.Parse(s);
    }

    public static class stringX
    {
        /// <summary>
        /// Determines whether the specified s is numeric.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>
        ///   <c>true</c> if the specified s is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(string s)
        {
            var i = 0;
            if (s[i] == '-') i++;
            var dot = false;
            for (; i < s.Length; i++)
            {
                var c = s[i];
                if (!char.IsDigit(c))
                {
                    if (c == '.' && !dot) { dot = true; continue; }
                    return false;
                }
            }
            return true;
        }

        public static int MD5Checksum(string s)
        {
            using var md5 = MD5.Create();
            var digest = md5.ComputeHash(Encoding.ASCII.GetBytes(s));
            return digest[0] ^ digest[1] ^ digest[2] ^ digest[3];
        }
        public static int MD5Checksum(byte[] buffer)
        {
            using var md5 = MD5.Create();
            var digest = md5.ComputeHash(buffer);
            return digest[0] ^ digest[1] ^ digest[2] ^ digest[3];
        }

        public static void sscanf<T1, T2>(string s, string fmt, out T1 t1, out T2 t2)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4, T5>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4, T5, T6>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4, T5, T6, T7>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4, T5, T6, T7, T8>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7, out T8 t8)
        {
            throw new NotImplementedException();
        }
        public static void sscanf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7, out T8 t8, out T9 t9)
        {
            throw new NotImplementedException();
        }
    }

    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        #region List

        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
        }

        public static int AddUnique<T>(this List<T> source, T item)
        {
            var index = source.FindIndex(x => x.Equals(item));
            if (index < 0)
                index = source.Add_(item);
            return index;
        }

        public static void SetNum<T>(this List<T> source, int newNum, bool resize = true)
        {
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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.NumericsX
{
    public static class intX
    {
        public static int Parse(string s) => int.Parse(s);
    }

    public static class floatX
    {
        public static float Parse(string s) => float.Parse(s);
    }

    public static class stringX
    {
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

        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

        #region Dictionary

        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue = default)
            => source.TryGetValue(key, out var z) ? z : defaultValue;

        public static void SetFloat(this Dictionary<string, string> source, string key, float val) => source[key] = $"{val}";
        public static void SetInt(this Dictionary<string, string> source, string key, int val) => source[key] = $"{val}";
        public static void SetBool(this Dictionary<string, string> source, string key, bool val) => source[key] = $"{(val ? 1 : 0)}";
        public static void SetVec2(this Dictionary<string, string> source, string key, Vector2 val) => source[key] = $"{val}";
        public static void SetVector(this Dictionary<string, string> source, string key, Vector3 val) => source[key] = $"{val}";
        public static void SetVec4(this Dictionary<string, string> source, string key, Vector4 val) => source[key] = $"{val}";
        public static void SetAngles(this Dictionary<string, string> source, string key, Angles val) => source[key] = $"{val}";
        public static void SetMatrix(this Dictionary<string, string> source, string key, Matrix3x3 val) => source[key] = $"{val}";

        public static bool TryGetString(this Dictionary<string, string> source, string key, string defaultValue, out string o) { var r = source.TryGetValue(key, out o); if (!r) o = defaultValue; return r; }
        public static bool TryGetFloat(this Dictionary<string, string> source, string key, string defaultValue, out float o) { var r = source.TryGetValue(key, out var z); o = floatX.Parse(r ? z : defaultValue); return r; }
        public static bool TryGetInt(this Dictionary<string, string> source, string key, string defaultValue, out int o) { var r = source.TryGetValue(key, out var z); o = intX.Parse(r ? z : defaultValue); return r; }
        public static bool TryGetBool(this Dictionary<string, string> source, string key, string defaultValue, out bool o) { var r = source.TryGetValue(key, out var z); o = intX.Parse(r ? z : defaultValue) != 0; return r; }
        public static bool TryGetVec2(this Dictionary<string, string> source, string key, string defaultValue, out Vector2 o) { var r = source.TryGetValue(key, out var z); o = new(); stringX.sscanf(r ? z : defaultValue ?? "0 0", "%f %f", out o.x, out o.y); return r; }
        public static bool TryGetVector(this Dictionary<string, string> source, string key, string defaultValue, out Vector3 o) { var r = source.TryGetValue(key, out var z); o = new(); stringX.sscanf(r ? z : defaultValue ?? "0 0 0", "%f %f %f", out o.x, out o.y, out o.z); return r; }
        public static bool TryGetVec4(this Dictionary<string, string> source, string key, string defaultValue, out Vector4 o) { var r = source.TryGetValue(key, out var z); o = new(); stringX.sscanf(r ? z : defaultValue ?? "0 0 0 0", "%f %f %f %f", out o.x, out o.y, out o.z, out o.w); return r; }
        public static bool TryGetAngles(this Dictionary<string, string> source, string key, string defaultValue, out Angles o) { var r = source.TryGetValue(key, out var z); o = new(); stringX.sscanf(r ? z : defaultValue ?? "0 0 0", "%f %f %f", out o.pitch, out o.yaw, out o.roll); return r; }
        public static bool TryGetMatrix(this Dictionary<string, string> source, string key, string defaultValue, out Matrix3x3 o)
        {
            var r = source.TryGetValue(key, out var z);
            o = Matrix3x3.identity;
            stringX.sscanf(r ? z : defaultValue ?? "1 0 0 0 1 0 0 0 1", "%f %f %f %f %f %f %f %f %f",
                out o[0].x, out o[0].y, out o[0].z,
                out o[1].x, out o[1].y, out o[1].z,
                out o[2].x, out o[2].y, out o[2].z);
            return r;
        }

        public static string GetString(this Dictionary<string, string> source, string key, string defaultValue = "") => source.TryGetValue(key, out var o) ? o : default;
        public static float GetFloat(this Dictionary<string, string> source, string key, string defaultValue = "0") => floatX.Parse(source.TryGetValue(key, out var o) ? o : defaultValue);
        public static int GetInt(this Dictionary<string, string> source, string key, string defaultValue = "0") => intX.Parse(source.TryGetValue(key, out var o) ? o : defaultValue);
        public static bool GetBool(this Dictionary<string, string> source, string key, string defaultValue = "0") => intX.Parse(source.TryGetValue(key, out var o) ? o : defaultValue) != 0;
        public static Vector2 GetVec2(this Dictionary<string, string> source, string key, string defaultValue = null) => source.TryGetVec2(key, default, out var z) ? z : default;
        public static Vector3 GetVector(this Dictionary<string, string> source, string key, string defaultValue = null) => source.TryGetVector(key, default, out var z) ? z : default;
        public static Vector4 GetVec4(this Dictionary<string, string> source, string key, string defaultValue = null) => source.TryGetVec4(key, default, out var z) ? z : default;
        public static Angles GetAngles(this Dictionary<string, string> source, string key, string defaultValue = null) => source.TryGetAngles(key, default, out var z) ? z : default;
        public static Matrix3x3 GetMatrix(this Dictionary<string, string> source, string key, string defaultValue = null) => source.TryGetMatrix(key, default, out var z) ? z : default;

        #endregion

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

        #region String

        public static string StripTrailingWhitespace(this string source)
        {
            return source;
        }

        public static string RemoveColors(this string source)
        {
            return source;
        }

        public static int LengthWithoutColors(this string source)
        {
            return 0;
        }

        #endregion

        public static string TrimStart(this string source, string s)
        {
            throw new NotImplementedException();
            //var l = s.Length;
            //if (l > 0)
            //    while (!Cmpn(s, l))
            //    {
            //        memmove(data, data + l, len - l + 1);
            //        len -= l;
            //    }
        }

        public static string TrimEnd(this string source, string s)
        {
            throw new NotImplementedException();
            //var l = s.Length;
            //if (l > 0)
            //{
            //    while ((len >= l) && !Cmpn(s, data + len - l, l))
            //    {
            //        len -= l;
            //        data[len] = '\0';
            //    }
            //}
        }

        public static string Trim(this string source, string s)
            => source.TrimStart(s).TrimEnd(s);

        // Returns true if the string conforms the given filter.
        // Several metacharacter may be used in the filter.
        // *          match any string of zero or more characters
        // ?          match any single character
        // [abc...]   match any of the enclosed characters; a hyphen can be used to specify a range (e.g. a-z, A-Z, 0-9)
        public static unsafe bool Filter(this string source, string match, bool caseSensitive)
        {
            StringBuilder buf = new(); int i, index; bool found;

            fixed (char* sourcep = source, matchp = match)
            {
                char* filter = sourcep, name = matchp;
                while (*filter != 0)
                {
                    if (*filter == '*')
                    {
                        filter++;
                        buf.Clear();
                        for (i = 0; *filter != 0; i++)
                        {
                            if (*filter == '*' || *filter == '?' || (*filter == '[' && *(filter + 1) != '['))
                                break;
                            buf.Append(*filter);
                            if (*filter == '[')
                                filter++;
                            filter++;
                        }
                        if (buf.Length != 0)
                        {
                            index = new string(name).IndexOf(buf.ToString(), caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                            if (index == -1)
                                return false;
                            name += index + buf.Length;
                        }
                    }
                    else if (*filter == '?')
                    {
                        filter++;
                        name++;
                    }
                    else if (*filter == '[')
                    {
                        if (*(filter + 1) == '[')
                        {
                            if (*name != '[')
                                return false;
                            filter += 2;
                            name++;
                        }
                        else
                        {
                            filter++;
                            found = false;
                            while (*filter != 0 && !found)
                            {
                                if (*filter == ']' && *(filter + 1) != ']')
                                    break;
                                if (*(filter + 1) == '-' && *(filter + 2) != 0 && (*(filter + 2) != ']' || *(filter + 3) == ']'))
                                {
                                    if (caseSensitive)
                                    {
                                        if (*name >= *filter && *name <= *(filter + 2))
                                            found = true;
                                    }
                                    else
                                    {
                                        if (char.ToUpperInvariant(*name) >= char.ToUpperInvariant(*filter) && char.ToUpperInvariant(*name) <= char.ToUpperInvariant(*(filter + 2)))
                                            found = true;
                                    }
                                    filter += 3;
                                }
                                else
                                {
                                    if (caseSensitive)
                                    {
                                        if (*filter == *name)
                                            found = true;
                                    }
                                    else
                                    {
                                        if (char.ToUpperInvariant(*filter) == char.ToUpperInvariant(*name))
                                            found = true;
                                    }
                                    filter++;
                                }
                            }
                            if (!found)
                                return false;
                            while (*filter != 0)
                            {
                                if (*filter == ']' && *(filter + 1) != ']')
                                    break;
                                filter++;
                            }
                            filter++;
                            name++;
                        }
                    }
                    else
                    {
                        if (caseSensitive)
                        {
                            if (*filter != *name)
                                return false;
                        }
                        else
                        {
                            if (char.ToUpperInvariant(*filter) != char.ToUpperInvariant(*name))
                                return false;
                        }
                        filter++;
                        name++;
                    }
                }
                return true;
            }
        }
    }
}
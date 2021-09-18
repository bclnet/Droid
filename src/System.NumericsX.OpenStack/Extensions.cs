using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    #region PathX

    public static class PathX
    {
        public static string BackSlashesToSlashes(string source)
            => source.Replace('\\', '/');

        public static string DefaultFileExtension(string source, string extension)
        {
            throw new NotImplementedException();
        }

        public static string StripFileExtension(string source)
        {
            throw new NotImplementedException();
        }

        public static string SetFileExtension(string source, string extension)
        {
            source = StripFileExtension(source);
            return extension[0] != '.' ? $"{source}.{extension}" : $"{source}{extension}";
        }

        public static string StripPath(string source)
        {
            throw new NotImplementedException();
        }

        public static string ExtractFileBase(string source, string baseName)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region stringX

    public static class stringX
    {
        #region Color

        static readonly Vector4[] ColorTable =
        {
            new(0f, 0f, 0f, 1f),
            new(1f, 0f, 0f, 1f), // S_COLOR_RED
	        new(0f, 1f, 0f, 1f), // S_COLOR_GREEN
	        new(1f, 1f, 0f, 1f), // S_COLOR_YELLOW
	        new(0f, 0f, 1f, 1f), // S_COLOR_BLUE
	        new(0f, 1f, 1f, 1f), // S_COLOR_CYAN
	        new(1f, 0f, 1f, 1f), // S_COLOR_MAGENTA
	        new(1f, 1f, 1f, 1f), // S_COLOR_WHITE
	        new(0.5f, 0.5f, 0.5f, 1f), // S_COLOR_GRAY
	        new(0f, 0f, 0f, 1f), // S_COLOR_BLACK
	        new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
        };

        public unsafe static bool IsColor(byte* s, void* till) => s[0] == '^' && s != till && s[1] != ' ';
        public static bool IsColor(byte[] s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(StringBuilder s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(string s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';

        public static int ColorIndex(int c) => c & 15;

        public static Vector4 ColorForIndex(int i) => ColorTable[i & 15];

        public static string RemoveColors(string s)
        {
            //char* d;
            //char* s2;
            //int c;

            //s2 = s;
            //d = s;
            //while ((c = *s2) != 0)
            //{
            //    if (IsColor(s, s2))
            //    {
            //        s2++;
            //    }
            //    else
            //    {
            //        *d++ = c;
            //    }
            //    s2++;
            //}
            //*d = '\0';

            return s;
        }

        public static int LengthWithoutColors(string s)
        {
            if (s == null)
                return 0;
            int sourceLength = s.Length, len = 0, p = 0;
            while (p < sourceLength)
            {
                if (IsColor(s, p)) { p += 2; continue; }
                p++; len++;
            }
            return len;
        }

        #endregion

        public static string StripQuotes(string s)
            => s[0] != '\"' ? s
            : s[^1] == '\"' ? s[1..^1]
            : s.Remove(0, 1);

        /// <summary>
        /// Safe strncpy that ensures a trailing zero
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="destsize">The destsize.</param>
        //public static void Copynz_(byte[] dest, byte[] src, int destsize)
        //{
        //    if (src != null) { Warning("Str::Copynz: NULL src"); return; }
        //    if (destsize < 1) { Warning("Str::Copynz: destsize < 1"); return; }
        //    Unsafe.CopyBlock(ref dest[0], ref src[0], (uint)destsize - 1);
        //    dest[destsize - 1] = 0;
        //}

        public static bool CharIsPrintable(char key)
        {
            throw new NotImplementedException();
        }

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

        #region Measue

        public enum MEASURE
        {
            SIZE = 0,
            BANDWIDTH
        }

        static string[,] MEASURE_Units =
        {
            { "B", "KB", "MB", "GB" },
            { "B/s", "KB/s", "MB/s", "GB/s" }
        };

        public static int BestUnit(out string s, string format, float value, MEASURE measure)
        {
            var unit = 1;
            while (unit <= 3 && (1 << (unit * 10) < value)) unit++;
            unit--;
            value /= 1 << (unit * 10);
            s = $"{string.Format(format, value)} {MEASURE_Units[(int)measure, unit]}";
            return unit;
        }

        public static void SetUnit(out string s, string format, float value, int unit, MEASURE measure)
        {
            value /= 1 << (unit * 10);
            s = $"{string.Format(format, value)} {MEASURE_Units[(int)measure, unit]}";
        }

        #endregion
    }

    #endregion

    public static class Extensions
    {
        #region Dictionary

        public static void Print(this Dictionary<string, string> source)
        {
            foreach (var kv in source)
                Printf($"{kv.Key} = {kv.Value}\n");
        }

        public static void SetDefaults(this Dictionary<string, string> source, Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
                if (!source.ContainsKey(kv.Key))
                    source[kv.Key] = kv.Value;
        }

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
        public static bool TryGetVec2(this Dictionary<string, string> source, string key, string defaultValue, out Vector2 o) { var r = source.TryGetValue(key, out var z); o = new(); TextScanFormatted.Scan(r ? z : defaultValue ?? "0 0", "%f %f", out o.x, out o.y); return r; }
        public static bool TryGetVector(this Dictionary<string, string> source, string key, string defaultValue, out Vector3 o) { var r = source.TryGetValue(key, out var z); o = new(); TextScanFormatted.Scan(r ? z : defaultValue ?? "0 0 0", "%f %f %f", out o.x, out o.y, out o.z); return r; }
        public static bool TryGetVec4(this Dictionary<string, string> source, string key, string defaultValue, out Vector4 o) { var r = source.TryGetValue(key, out var z); o = new(); TextScanFormatted.Scan(r ? z : defaultValue ?? "0 0 0 0", "%f %f %f %f", out o.x, out o.y, out o.z, out o.w); return r; }
        public static bool TryGetAngles(this Dictionary<string, string> source, string key, string defaultValue, out Angles o) { var r = source.TryGetValue(key, out var z); o = new(); TextScanFormatted.Scan(r ? z : defaultValue ?? "0 0 0", "%f %f %f", out o.pitch, out o.yaw, out o.roll); return r; }
        public static bool TryGetMatrix(this Dictionary<string, string> source, string key, string defaultValue, out Matrix3x3 o)
        {
            var r = source.TryGetValue(key, out var z);
            o = Matrix3x3.identity;
            TextScanFormatted.Scan(r ? z : defaultValue ?? "1 0 0 0 1 0 0 0 1", "%f %f %f %f %f %f %f %f %f",
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


        public static int FindIndex<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
        {
            var index = 0;
            foreach (var kv in source)
            {
                if (predicate(kv))
                    return index;
                index++;
            }
            return -1;
        }

        public static KeyValuePair<string, TValue> MatchPrefix<TValue>(this Dictionary<string, TValue> source, string prefix, KeyValuePair<string, TValue> lastMatch = default)
        {
            Debug.Assert(prefix != null);
            var start = -1;
            if (lastMatch.Key != null)
            {
                start = source.FindIndex(x => x.Key == lastMatch.Key);
                Debug.Assert(start >= 0);
                if (start < 1) start = 0;
            }
            var keys = source.Keys.Skip(start + 1);
            foreach (var kv in source.Skip(start + 1))
                if (kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return kv;
            return default;
        }

        #endregion

        #region String

        public static string StripLeading(this string source, string leading)
        {
            return source;
        }

        public static string StripTrailingWhitespace(this string source)
        {
            return source;
        }

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

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using static Droid.Core.Lib;

namespace Droid.Core
{
    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

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

    public static class StringX
    {
        public static bool IsColor(byte[] s, int offset)
            => s[offset + 0] == '^' && s[offset + 1] != '\0' && s[offset + 1] != ' ';
        public static bool IsColor(StringBuilder s, int offset)
            => s[offset + 0] == '^' && s[offset + 1] != '\0' && s[offset + 1] != ' ';
        public static bool IsColor(string s, int offset)
            => s[offset + 0] == '^' && s[offset + 1] != '\0' && s[offset + 1] != ' ';

        public static unsafe string FloatArrayToString(float* array, int length, int precision)
        {
            //static int index = 0;
            //static char str[4][16384];  // in case called by nested functions
            //int i, n;
            //char format[16], *s;

            //// use an array of string so that multiple calls won't collide
            //s = str[index];
            //index = (index + 1) & 3;

            //idStr::snPrintf(format, sizeof(format), "%%.%df", precision);
            //n = idStr::snPrintf(s, sizeof(str[0]), format, array[0]);
            //if (precision > 0)
            //{
            //    while (n > 0 && s[n - 1] == '0') s[--n] = '\0';
            //    while (n > 0 && s[n - 1] == '.') s[--n] = '\0';
            //}
            //idStr::snPrintf(format, sizeof(format), " %%.%df", precision);
            //for (i = 1; i < length; i++)
            //{
            //    n += idStr::snPrintf(s + n, sizeof(str[0]) - n, format, array[i]);
            //    if (precision > 0)
            //    {
            //        while (n > 0 && s[n - 1] == '0') s[--n] = '\0';
            //        while (n > 0 && s[n - 1] == '.') s[--n] = '\0';
            //    }
            //}
            //return s;
            return "STRING";
        }

        /// <summary>
        /// Safe strncpy that ensures a trailing zero
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="destsize">The destsize.</param>
        public static void Copynz(byte[] dest, byte[] src, int destsize)
        {
            if (src != null)
            {
                common.Warning("Str::Copynz: NULL src");
                return;
            }
            if (destsize < 1)
            {
                common.Warning("Str::Copynz: destsize < 1");
                return;
            }
            Unsafe.CopyBlock(ref dest[0], ref src[0], (uint)destsize - 1);
            dest[destsize - 1] = 0;
        }
    }
}
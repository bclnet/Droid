using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.NumericsX.Core
{
    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue = default)
            => source.TryGetValue(key, out var z) ? z : defaultValue;

        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
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
using System;
using System.Diagnostics;

namespace Droid.Core
{
    public static class StringX
    {
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
    }
}
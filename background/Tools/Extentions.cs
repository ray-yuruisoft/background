using System;
using System.Collections.Generic;
using System.Text;

namespace background.Tools
{
    public static class Extentions
    {
        public static string ToSameLength(this string source, int length, char c)
        {
            if (source.Length < length)
            {
                return source.PadRight(length - source.Length, c);
            }
            else
            {
                return source.Substring(0, length);
            }
        }
    }
}

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

        public static string ToLine(this string source,char c)
        {
            var width = 120;
            try
            {
                width = Console.WindowWidth;
            }
            catch
            {
                // ignore
            }
            var len = width - source.Length-10;
            if(len>0)
            {
              return source.PadRight(len, c);
            }
            return source;
        }


    /// <summary>
    /// 打印一整行word到控制台中
    /// </summary>
    /// <param name="word">打印的字符</param>
    public static void PrintLine(char word = '=')
    {
        var width = 120;

        try
        {
            width = Console.WindowWidth;
        }
        catch
        {
            // ignore
        }
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < width; ++i)
        {
            builder.Append(word);
        }

        Console.Write(builder.ToString());
    }

    }
}

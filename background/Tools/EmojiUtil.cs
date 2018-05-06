using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherAgentService2.Tool
{
    public class EmojiUtil
    {


        ///**
        // * emoji表情替换
        // *
        // * @param source 原字符串
        // * @param slipStr emoji表情替换成的字符串                
        // * @return 过滤后的字符串
        // */
        //public static String filterEmoji(String source, String slipStr)
        //{
        //    if (!string.IsNullOrWhiteSpace(source))
        //    {
        //        return source.Replace("[\\ud800\\udc00-\\udbff\\udfff\\ud800-\\udfff]", slipStr);
        //    }
        //    else
        //    {
        //        return source;
        //    }
        //}

        /**
          * 检测是否有emoji字符
          * @param source
          * @return 一旦含有就抛出
          */
        public static Boolean containsEmoji(String source)
        {
            char[] item = source.ToCharArray();
            for (int i = 0; i < source.Length; i++)
            {
                if (isEmojiCharacter(item[i]))
                    return true; //do nothing，判断到了这里表明，确认有表情字符
            }
            return false;
        }
        private static Boolean isEmojiCharacter(char codePoint)
        {
            return (codePoint == 0x0) ||
                    (codePoint == 0x9) ||
                    (codePoint == 0xA) ||
                    (codePoint == 0xD) ||
                    ((codePoint >= 0x20) && (codePoint <= 0xD7FF)) ||
                    ((codePoint >= 0xE000) && (codePoint <= 0xFFFD)) ||
                    ((codePoint >= 0x10000) && (codePoint <= 0x10FFFF));
        }
        /**
         * 过滤emoji 或者 其他非文字类型的字符
         * @param source
         * @return
         */
        public static String filterEmoji(String source)
        {
            if (!containsEmoji(source))
                return source;//如果不包含，直接返回
            //到这里铁定包含
            StringBuilder buf = null;
            char[] item = source.ToCharArray();
            for (int i = 0; i < source.Length; i++)
            {
                char codePoint = item[i];
                if (isEmojiCharacter(codePoint))
                {
                    if (buf == null)
                        buf = new StringBuilder(source.Length);
                    buf.Append(codePoint);
                }
            }
            if (buf == null)
                return source;//如果没有找到 emoji表情，则返回源字符串
            else
            {
                if (buf.Length == source.Length)
                {
                    buf = null;//这里的意义在于尽可能少的toString，因为会重新生成字符串
                    return source;
                }
                else
                    return buf.ToString();
            }

        }
    }
}

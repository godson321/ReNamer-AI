using System;
using System.Text;
using Microsoft.VisualBasic;

namespace ReNamer.Helpers;

/// <summary>
/// 汉字拼音帮助类，支持简繁体中文
/// 使用Unicode编码范围判断和首字母映射表
/// </summary>
public static class PinyinHelper
{
    /// <summary>
    /// 获取字符串中第N个中文字符的拼音首字母
    /// </summary>
    /// <param name="text">输入文本</param>
    /// <param name="chineseIndex">第几个中文字符（从1开始）</param>
    /// <param name="upperCase">是否大写</param>
    /// <returns>拼音首字母，如果找不到返回空字符串</returns>
    public static string GetPinyinFirstLetter(string text, int chineseIndex, bool upperCase = true)
    {
        if (string.IsNullOrEmpty(text) || chineseIndex < 1)
            return "";

        int count = 0;
        foreach (char c in text)
        {
            if (IsChinese(c))
            {
                count++;
                if (count == chineseIndex)
                {
                    char letter = GetFirstLetter(c);
                    return upperCase ? letter.ToString().ToUpper() : letter.ToString().ToLower();
                }
            }
        }
        return "";
    }

    /// <summary>
    /// 判断字符是否为中文（包括简繁体）
    /// </summary>
    public static bool IsChinese(char c)
    {
        // CJK统一汉字基本区: U+4E00 - U+9FFF
        // CJK统一汉字扩展A区: U+3400 - U+4DBF
        // CJK统一汉字扩展B区: U+20000 - U+2A6DF (需要surrogate pair，这里简化处理)
        // CJK兼容汉字: U+F900 - U+FAFF
        return (c >= 0x4E00 && c <= 0x9FFF) ||
               (c >= 0x3400 && c <= 0x4DBF) ||
               (c >= 0xF900 && c <= 0xFAFF);
    }

    /// <summary>
    /// 获取汉字的拼音首字母
    /// 使用GB2312编码区间判断法，适用于大多数简繁体汉字
    /// </summary>
    public static char GetFirstLetter(char c)
    {
        // 非中文字符返回原字符
        if (!IsChinese(c))
            return c;
        // 先尝试原字符
        var letter = TryGetFirstLetterByEncoding(c, "GB2312");
        if (letter != '#') return letter;
        letter = TryGetFirstLetterByEncoding(c, "GBK");
        if (letter != '#') return letter;

        // 繁体字回退：先转简体再尝试
        var simplified = TryToSimplified(c);
        if (simplified != c)
        {
            letter = TryGetFirstLetterByEncoding(simplified, "GB2312");
            if (letter != '#') return letter;
            letter = TryGetFirstLetterByEncoding(simplified, "GBK");
            if (letter != '#') return letter;
        }

        // 如果都失败了，返回'#'表示未知
        return '#';
    }

    private static char TryGetFirstLetterByEncoding(char c, string encodingName)
    {
        try
        {
            Encoding enc = Encoding.GetEncoding(encodingName);
            byte[] bytes = enc.GetBytes(c.ToString());
            if (bytes.Length == 2)
            {
                int code = bytes[0] * 256 + bytes[1];
                return GetLetterByGBCode(code);
            }
        }
        catch
        {
            // ignore and fallback
        }
        return '#';
    }

    private static char TryToSimplified(char c)
    {
        try
        {
            // 0x0804 = zh-CN
            var converted = Strings.StrConv(c.ToString(), VbStrConv.SimplifiedChinese, 0x0804);
            if (!string.IsNullOrEmpty(converted))
                return converted[0];
        }
        catch
        {
            // ignore and fallback
        }
        return c;
    }

    /// <summary>
    /// 根据GB2312/GBK编码值获取拼音首字母
    /// 基于汉字按拼音排序的特性
    /// </summary>
    private static char GetLetterByGBCode(int code)
    {
        // GB2312汉字区拼音首字母分界值
        if (code >= 45217 && code <= 45252) return 'A';
        if (code >= 45253 && code <= 45760) return 'B';
        if (code >= 45761 && code <= 46317) return 'C';
        if (code >= 46318 && code <= 46825) return 'D';
        if (code >= 46826 && code <= 47009) return 'E';
        if (code >= 47010 && code <= 47296) return 'F';
        if (code >= 47297 && code <= 47613) return 'G';
        if (code >= 47614 && code <= 48118) return 'H';
        if (code >= 48119 && code <= 49061) return 'J';
        if (code >= 49062 && code <= 49323) return 'K';
        if (code >= 49324 && code <= 49895) return 'L';
        if (code >= 49896 && code <= 50370) return 'M';
        if (code >= 50371 && code <= 50613) return 'N';
        if (code >= 50614 && code <= 50621) return 'O';
        if (code >= 50622 && code <= 50905) return 'P';
        if (code >= 50906 && code <= 51386) return 'Q';
        if (code >= 51387 && code <= 51445) return 'R';
        if (code >= 51446 && code <= 52217) return 'S';
        if (code >= 52218 && code <= 52697) return 'T';
        if (code >= 52698 && code <= 52979) return 'W';
        if (code >= 52980 && code <= 53688) return 'X';
        if (code >= 53689 && code <= 54480) return 'Y';
        if (code >= 54481 && code <= 55289) return 'Z';

        // 扩展区域（GBK）
        if (code >= 55290 && code <= 65535)
        {
            // GBK扩展汉字，使用更细的划分
            return GetLetterByGBKExtended(code);
        }

        return '#';
    }

    /// <summary>
    /// GBK扩展区汉字拼音首字母（包含更多繁体字）
    /// </summary>
    private static char GetLetterByGBKExtended(int code)
    {
        // 这是一个简化的映射，覆盖常用繁体字
        // 实际上GBK扩展区的汉字没有严格按拼音排序
        // 这里返回'#'，后续可以通过字典表扩展
        return '#';
    }

    /// <summary>
    /// 统计字符串中的中文字符数量
    /// </summary>
    public static int CountChinese(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        int count = 0;
        foreach (char c in text)
        {
            if (IsChinese(c)) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取字符串中所有中文字符的拼音首字母
    /// </summary>
    public static string GetAllPinyinFirstLetters(string text, bool upperCase = true)
    {
        if (string.IsNullOrEmpty(text)) return "";
        
        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (IsChinese(c))
            {
                char letter = GetFirstLetter(c);
                sb.Append(upperCase ? char.ToUpper(letter) : char.ToLower(letter));
            }
        }
        return sb.ToString();
    }
}

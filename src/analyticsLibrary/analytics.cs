using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace analyticsLibrary
{
    public static class analytics
    {
        public static string compress(this string value, string compress)
            => Regex.Replace(value, string.Format("([{0}]+)", compress), compress);

        public static string compress(this string value)
        {
            if (value == null || value.Length == 0) return value;

            var compressed = new StringBuilder();
            var aValue = value.ToCharArray();
            char lastChar = aValue[0];
            char currChar = aValue[0];
            for (var i = 0; i < aValue.Length; i++)
            {
                currChar = aValue[i];
                if (i == 0)
                {
                    compressed.Append(currChar);
                }
                else
                {
                    if (currChar != lastChar) compressed.Append(currChar);
                    lastChar = currChar;
                }
            }

            return compressed.ToString();
        }

        public static string titleCase(this string value)
            => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLower());

        public static string wordReplace(this string value, string word, string replace)
            => Regex.Replace(value, word, replace);
    }
}
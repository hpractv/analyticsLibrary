using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace analyticsLibrary.library
{
    public static class presentation
    {
        private const string hexColor = @"^#(((?<r>[a-fA-F\d]{2}?)(?<g>[a-fA-F\d]{2}?)(?<b>[a-fA-F\d]{2}?))|((?<r>[a-fA-F\d]{1}?)(?<g>[a-fA-F\d]{1}?)(?<b>[a-fA-F\d]{1}?)))$";
        private static Regex colorCheck = new Regex(hexColor, RegexOptions.Singleline | RegexOptions.Compiled);
        public static string[] interpolateColors(string color1, string color2, int steps)
        {
            var colorValues = new int[steps][];
            for (int i = 0; i < steps; i++) colorValues[i] = new int[3] { 0, 0, 0 };

            var validColors = colorCheck.IsMatch(color1) && colorCheck.IsMatch(color2);

            if (!validColors) throw new ApplicationException($"Invalid color ranges: '{color1}' and/or '{color2}'.");

            var c1Matches = colorCheck.Match(color1);
            var c2Matches = colorCheck.Match(color2);

            var r1 = pValue(c1Matches.Groups["r"].Value);
            var g1 = pValue(c1Matches.Groups["g"].Value);
            var b1 = pValue(c1Matches.Groups["b"].Value);
            var r2 = pValue(c2Matches.Groups["r"].Value);
            var g2 = pValue(c2Matches.Groups["g"].Value);
            var b2 = pValue(c2Matches.Groups["b"].Value);

            var rDelta = (double)(r2 - r1);
            var gDelta = (double)(g2 - g1);
            var bDelta = (double)(b2 - b1);
            
            for (int i = 0; i < steps - 1; i++)
            {

                colorValues[i] = new int[] {
                    r1 + (int)((rDelta * i)/steps),
                    g1 + (int)((gDelta * i)/steps),
                    b1 + (int)((bDelta * i)/steps)
                };

            }
            colorValues[steps - 1] = new int[] { r2, g2, b2 };

            return colorValues.Select(v => $"#{hValue(v[0])}{hValue(v[1])}{hValue(v[2])}").ToArray();

            //utilities
            int pValue(string hex)
            {
                if (hex.Length == 1)
                    return int.Parse($"{hex}{hex}", System.Globalization.NumberStyles.HexNumber);

                return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            string hValue(int p)
            {
                var hex = p.ToString("x");
                return (hex.Length == 1 ? $"0{hex}" : hex);
            }
        }
    }
}

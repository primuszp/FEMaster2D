using System;
using System.Globalization;

namespace FEMaster.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.CurrentCulture;

        public static int ToInt(this string str)
        {
            return int.Parse(str.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static double ToDouble(this string str, double ratio = 1.0, int digits = 15)
        {
            str = str.Trim().Replace(',', '.');
            return Math.Round(double.Parse(str, NumberStyles.Float, CultureInfo.InvariantCulture) * ratio, digits);
        }

        public static string RemoveExtraSpaces(this string str)
        {
            while (str.Contains("  "))
            {
                str = str.Replace("  ", " ");
            }

            if (str != string.Empty)
            {
                str = str[0] == ' ' ? str.Substring(1) : str;
                str = str.EndsWith(" ", StringComparison.Ordinal) ? str.Remove(str.Length - 1) : str;
            }

            return str;
        }
    }
}
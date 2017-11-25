using System;
using System.Globalization;

namespace Pavexpert.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.CurrentCulture;

        public static int ToInt(this string str)
        {
            return int.Parse(str, NumberStyles.Any);
        }

        public static double ToDouble(this string str, double ratio = 1.0, int digits = 15)
        {
            string separator = CultureInfo.NumberFormat.CurrencyDecimalSeparator;

            int index = Math.Max(str.IndexOf('.'), str.IndexOf(','));

            if (index >= 0 && !char.IsDigit(str[index]))
            {
                str = str.Replace(str[index].ToString(), separator);
            }

            return Math.Round(double.Parse(str, NumberStyles.Float) * ratio, digits);
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
using System;
using System.Globalization;

namespace Pavexpert.Core.Extensions
{
    public static class StringExtensions
    {
        public static int ToInt(this string text)
        {
            return Convert.ToInt32(ToDouble(text));
        }

        public static double ToDouble(this string text)
        {
            double result;
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (!text.Contains(separator))
            {
                if (separator == ".") text = text.Replace(',', '.');
                if (separator == ",") text = text.Replace('.', ',');
            }

            return double.TryParse(text, out result) ? result : double.NaN;
        }
    }
}

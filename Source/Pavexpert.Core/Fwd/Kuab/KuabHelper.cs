using System;
using System.Collections.Generic;
using System.IO;
using Pavexpert.Core.Extensions;

namespace Pavexpert.Core.Fwd.Kuab
{
    public static class KuabHelper
    {
        private static readonly List<string> Lines = new List<string>();

        public static void ConvertUnitsTo(Stream input, Stream output)
        {
            Lines.Clear();

            using (StreamReader reader = new StreamReader(input))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    PlateRadius(ref line, "mm", 10.0d);
                    ImpactLoad(ref line, "kn", 0.010d);
                    SensorDistance(ref line, "mm", 10.0d);
                    DropHeaderUnits(ref line, "kn", "mc");
                    Drops(ref line, "kn", 0.010d);

                    Lines.Add(line);
                }
            }

            Save(output);
        }

        private static void Save(Stream output)
        {
            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (string line in Lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private static void PlateRadius(ref string line, string unit, double factor)
        {
            if (line.Contains("IPlate Radius"))
            {
                string[] buffer = line.Substring(20).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string value = Convert.ToString(buffer[0].ToDouble() * factor);
                line = line.Replace(buffer[0], value.Replace(",", "."));
                line = line.Replace(buffer[1], " (" + unit + ")");
            }
        }

        private static void ImpactLoad(ref string line, string unit, double factor)
        {
            if (line.Contains("IImpact Load"))
            {
                string[] buffer = line.Substring(20).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < buffer.Length - 1; i++)
                {
                    string value = Math.Round(buffer[i].ToDouble() * factor, 2).ToString("0.0");
                    line = line.Replace(buffer[i], value.Replace(",", "."));
                }

                line = line.Replace(buffer[buffer.Length - 1], unit);
            }
        }

        private static void SensorDistance(ref string line, string unit, double factor)
        {
            if (line.Contains("ISensor Distance"))
            {
                string[] buffer = line.Substring(20).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] values = new string[7];

                for (int i = 0; i < buffer.Length - 1; i++)
                {
                    values[i] = Math.Round(buffer[i].ToDouble() * factor, 2).ToString("0");
                }

                line = string.Format("ISensor Distance  :      {0}    {1}    {2}    {3}    {4}    {5}   {6} ({7})", values[0], values[1], values[2],
                    values[3], values[4], values[5], values[6], unit);
            }
        }

        private static void DropHeaderUnits(ref string line, string unit1, string unit2)
        {
            if (line.Contains("kgf") && line.Contains("µm"))
            {
                line = line.Replace("kgf", " " + unit1);
                line = line.Replace("µm", unit2);
            }
        }

        private static void Drops(ref string line, string unit, double factor)
        {
            if (line.IndexOf("D", StringComparison.Ordinal) == 0)
            {
                string[] buffer = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string value = Math.Round(buffer[3].ToDouble() * factor, 2).ToString("0.0");
                line = line.Replace(buffer[3], value);
            }
        }
    }
}

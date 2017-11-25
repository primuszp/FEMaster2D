using System;
using System.Collections.Generic;
using System.IO;
using Pavexpert.Core.Results;
using Pavexpert.Core.Pavement;
using Pavexpert.Core.Extensions;

namespace Pavexpert.Core.Bisar
{
    public sealed class BisarDocument
    {
        #region Members

        private string token = string.Empty;
        private BisarRegion region = BisarRegion.Title;
        private PointResult result;
        private BisarSystem system;

        #endregion

        #region Properties

        public string Title { get; set; }

        public DateTime DateTime { get; private set; }

        public List<BisarSystem> Systems { get; set; }

        public bool IsIncomplete { get; private set; }

        #endregion

        #region Constructors

        public BisarDocument()
        {
            Title = "TITLE";
            Systems = new List<BisarSystem>();
            DateTime = DateTime.Now;
        }

        #endregion

        #region BISAR Output Reader Methods

        public void Read(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = string.Empty;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.RemoveExtraSpaces();

                    if (line != string.Empty)
                    {
                        if (line.Equals("***** T H E E N D *****"))
                        {
                            region = BisarRegion.End;
                        }

                        Parser(line);
                    }
                }
                IsIncomplete = region != BisarRegion.End;
            }
        }

        private void Parser(string line)
        {
            string[] buffer = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (buffer.Length == 0) return;

            switch (region)
            {
                case BisarRegion.Title:
                    ParseTitle(buffer);
                    break;
                case BisarRegion.DateTime:
                    ParseDateTime(buffer);
                    break;
                case BisarRegion.System:
                    ParseSystem(buffer);
                    break;
                case BisarRegion.Layers:
                    ParseLayers(buffer);
                    break;
                case BisarRegion.Loads:
                    ParseLoads(buffer);
                    break;
                case BisarRegion.Positions:
                    ParsePositions(buffer);
                    break;
                case BisarRegion.End:
                    ParseEnd(buffer);
                    break;
            }
        }

        private void ParseEnd(string[] buffer)
        {
            if (buffer[0].Equals("*****") && buffer[7].Equals("*****"))
            {
                if (system != null)
                {
                    Systems.Add(system);
                }
            }
        }

        private void ParseLoads(string[] buffer)
        {
            if (buffer[0].Equals("**************************************************"))
            {
                token = string.Empty;
                region = BisarRegion.Positions;
            }
            else if (buffer[0].Equals("LOAD") && token == string.Empty)
            {
                token = buffer[0];
            }
            else if (buffer[0].Equals("NUMBER") && token == "LOAD")
            {
                token = token + "+" + buffer[0];
            }
            else if (token == "LOAD+NUMBER")
            {
                var number = buffer[0].ToInt();
                var xcoord = buffer[4].ToDouble(0.0254, 3);         // In -> m
                var ycoord = buffer[5].ToDouble(0.0254, 3);         // In -> m
                var radius = buffer[3].ToDouble(0.0254, 3);         // In -> m

                var angle = buffer[6].ToDouble(180.0 / Math.PI, 2); // Rad -> degrees
                var vload = buffer[1].ToDouble(0.0044482216000, 2); // Lb  -> kN
                var hstress = buffer[2].ToDouble(0.00689475728, 2); // PSI -> MPa
                var hload = Math.Round(hstress * (1000 * radius * radius * Math.PI), 3); // kN

                system.Loads.Add(new Load(number, xcoord, ycoord, vload, radius, hload, angle));
            }
        }

        private void ParseTitle(string[] buffer)
        {
            Title = buffer[0];
            region = BisarRegion.DateTime;

        }

        private void ParseLayers(string[] buffer)
        {
            if (buffer[0].Equals("LAYER") && token == string.Empty)
            {
                token = buffer[0];

                if (buffer[5] == "REDUCED")
                    system.SpringCompliance = SpringComplianceType.Reduced;
                if (buffer[5] == "INTERFACE")
                    system.SpringCompliance = SpringComplianceType.Interface;
            }
            else if (buffer[0].Equals("NUMBER") && token == "LAYER")
            {
                token = token + "+" + buffer[0];
            }
            else if (token == "LAYER+NUMBER")
            {
                Layer layer = new Layer { Number = buffer[0].ToInt() };

                if (buffer.Length == 6)
                {
                    if (buffer[1] == "ROUGH")
                        system.CalculationMethod = CalculationMethodType.Rough;
                    if (buffer[1] == "SMOOTH")
                        system.CalculationMethod = CalculationMethodType.Smooth;

                    layer.Material.Modulus = buffer[2].ToDouble(0.00689475728, 2);  // PSI -> MPa
                    layer.Material.Poisson = buffer[3].ToDouble();
                    layer.Thickness = buffer[4].ToDouble(0.0254, 0);                // In -> m
                    layer.Roughness = buffer[5].ToDouble();                         // TODO !!!
                }
                else if (buffer.Length == 3)
                {
                    layer.Material.Modulus = buffer[1].ToDouble(0.00689475728, 2); // PSI -> MPa
                    layer.Material.Poisson = buffer[2].ToDouble();
                    region = BisarRegion.Loads;
                    token = string.Empty;
                }

                system.Layers.Add(layer);
            }
        }

        private void ParseSystem(string[] buffer)
        {
            if (buffer[0].Equals("SYSTEM") && buffer[1].Equals("NUMBER"))
            {
                if (system != null)
                {
                    Systems.Add(system);
                }

                system = new BisarSystem { Number = buffer[2].ToInt() };
                region = BisarRegion.Layers;
            }
            else if (buffer[0].Equals("**************************************************"))
            {
                region = BisarRegion.Positions;
            }
        }

        private void ParseDateTime(string[] buffer)
        {
            if (buffer[0].Equals("DATE====>"))
            {
                var date = buffer[1].Split(new char[] { '/' });

                if (date.Length == 3)
                {
                    var dd = date[0].ToInt();
                    var mm = date[1].ToInt();
                    var yy = date[2].ToInt();

                    DateTime = new DateTime(yy, mm, dd);
                }
            }
            else if (buffer[0].Equals("TIME====>"))
            {
                var time = buffer[1].Split(new char[] { ':' });

                if (time.Length == 3)
                {
                    var hh = time[0].ToDouble();
                    var mm = time[1].ToDouble();
                    var ss = time[2].ToDouble();

                    DateTime = DateTime.AddHours(hh);
                    DateTime = DateTime.AddMinutes(mm);
                    DateTime = DateTime.AddSeconds(ss);

                    region = BisarRegion.System;
                }
            }
        }

        private void ParsePositions(string[] buffer)
        {
            if (buffer[0].Equals("POSTION") && buffer.Length == 6)
            {
                result = new PointResult
                {
                    Number = buffer[2].ToInt(),
                    LayerNumber = buffer[5].ToInt()
                };
            }
            else if (buffer[0].Equals("X-COORDINATE==>") && result != null)
            {
                result.X = buffer[1].ToDouble();
            }
            else if (buffer[0].Equals("Y-COORDINATE==>") && result != null)
            {
                result.Y = buffer[1].ToDouble();
            }
            else if (buffer[0].Equals("Z-DEPTH=======>") && result != null)
            {
                result.Z = buffer[1].ToDouble();
            }
            else if (buffer[0].Equals("STRESS") && result != null)
            {
                // PSI -> MPa
                var stressXX = buffer[1].ToDouble(0.0068947591);
                var stressYY = buffer[2].ToDouble(0.0068947591);
                var stressZZ = buffer[3].ToDouble(0.0068947591);
                var stressYZ = buffer[4].ToDouble(0.0068947591);
                var stressXZ = buffer[5].ToDouble(0.0068947591);
                var stressXY = buffer[6].ToDouble(0.0068947591);

                result.SetStress(stressXX, stressYY, stressZZ, stressYZ, stressXZ, stressXY);
            }
            else if (buffer[0].Equals("STRAIN") && result != null)
            {
                // In/In -> μstrain
                var strainXX = buffer[1].ToDouble(1000000);
                var strainYY = buffer[2].ToDouble(1000000);
                var strainZZ = buffer[3].ToDouble(1000000);
                var strainYZ = buffer[4].ToDouble(1000000);
                var strainXZ = buffer[5].ToDouble(1000000);
                var strainXY = buffer[6].ToDouble(1000000);

                result.SetStrain(strainXX, strainYY, strainZZ, strainYZ, strainXZ, strainXY);
            }
            else if (buffer[0].Equals("DISPLT") && result != null)
            {
                // In -> μm
                var displtXX = buffer[1].ToDouble(25400);
                var displtYY = buffer[2].ToDouble(25400);
                var displtZZ = buffer[3].ToDouble(25400);

                result.SetDisplacement(displtXX, displtYY, displtZZ);
                system.Evaluations.Add(result);
                region = BisarRegion.System;
            }
        }

        #endregion

        #region BISAR Input Writer Methods

        public void Write(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.NewLine = "\r\n";
                WriteBisarTitle(writer);
                WriteNumberOfSystems(writer);

                foreach (BisarSystem bsystem in Systems)
                {
                    WriteNumberOfStrLayers(bsystem, writer);
                    WriteCalculationMethod(bsystem, writer);
                    WriteSpringCompliances(bsystem, writer);
                    WriteLayersOfStructure(bsystem, writer);
                    WriteWheelLoadOfSystem(bsystem, writer);
                    WriteSystemEvaluations(bsystem, writer);
                }
            }
        }

        private void WriteBisarTitle(StreamWriter writer)
        {
            string spaces = CalculateSpaces(72, Title);
            writer.WriteLine(Title + spaces);
        }

        private void WriteNumberOfSystems(StreamWriter writer)
        {
            string number = Convert.ToString(Systems.Count);
            string spaces = CalculateSpaces(12, number);
            writer.Write(spaces);
            writer.WriteLine(number);
        }

        private void WriteNumberOfStrLayers(BisarSystem bsystem, StreamWriter writer)
        {
            writer.Write(string.Format("  {0}", bsystem.Layers.Count));
        }

        private void WriteCalculationMethod(BisarSystem bsystem, StreamWriter writer)
        {
            switch (bsystem.CalculationMethod)
            {
                case CalculationMethodType.Rough:
                    // Rough Computational Procedure
                    writer.Write("  " + "0");
                    break;
                case CalculationMethodType.Smooth:
                    // Smooth Computational Procedure
                    writer.Write("  " + "1");
                    break;
            }
        }

        private void WriteSpringCompliances(BisarSystem bsystem, StreamWriter writer)
        {
            switch (bsystem.SpringCompliance)
            {
                case SpringComplianceType.Interface:
                    writer.WriteLine("  " + "0");
                    break;
                case SpringComplianceType.Reduced:
                    writer.WriteLine("  " + "1");
                    break;
            }
        }

        private void WriteLayersOfStructure(BisarSystem bsystem, StreamWriter writer)
        {
            string spaces = string.Empty;
            string modulus = string.Empty;
            string poisson = string.Empty;
            string thickness = string.Empty;
            string roughness = string.Empty;

			// for (int i = bsystem.Layers.Count - 1; i >= 0; i--)
			for (int i = 0; i < bsystem.Layers.Count; i++)
            {
                modulus = ConvertToImperial(bsystem.Layers[i].Material.Modulus, 145.0377, 2);
                poisson = ConvertToImperial(bsystem.Layers[i].Material.Poisson, 1.000000, 2);

                spaces = CalculateSpaces(12, modulus);
                writer.Write(spaces);
                writer.Write(modulus);

                spaces = CalculateSpaces(8, poisson);
                writer.Write(spaces);
                writer.Write(poisson);

                if (!bsystem.Layers[i].IsHalfSpace)
                {
                    //roughness = "1000000"; // Teljes elcsúszás

                    roughness = Convert.ToString(bsystem.Layers[i].Roughness * 1000000.0);
                    thickness = ConvertToImperial(bsystem.Layers[i].Thickness, 39.370, 2);

                    spaces = CalculateSpaces(10, thickness);
                    writer.Write(spaces);
                    writer.Write(thickness);

                    spaces = CalculateSpaces(10, roughness);
                    writer.Write(spaces);
                    writer.Write(roughness);
                }
                writer.WriteLine();
            }
        }

        private void WriteWheelLoadOfSystem(BisarSystem bsystem, StreamWriter writer)
        {
            writer.WriteLine(string.Format("  {0}", bsystem.Loads.Count));

            string nforce = string.Empty;
            string sforce = string.Empty;
            string radius = string.Empty;
            string coordX = string.Empty;
            string coordY = string.Empty;
            string spaces = string.Empty;
            string degree = string.Empty;

            for (int i = 0; i < bsystem.Loads.Count; i++)
            {
                Load load = bsystem.Loads[i];

                coordX = ConvertToImperial(load.X, 39.370, 2);
                coordY = ConvertToImperial(load.Y, 39.370, 2);
                radius = ConvertToImperial(load.Radius, 39.370, 2);
                sforce = ConvertToImperial(load.ShearLoad, 224.80, 2);
                degree = ConvertToImperial(load.ShearDirection, 1, 2);
                nforce = ConvertToImperial(load.NormalLoad, 224.8, 2);

                spaces = CalculateSpaces(12, nforce);
                writer.Write(spaces);
                writer.Write(nforce);

                spaces = CalculateSpaces(08, radius);
                writer.Write(spaces);
                writer.Write(radius);

                spaces = CalculateSpaces(10, coordX);
                writer.Write(spaces);
                writer.Write(coordX);

                spaces = CalculateSpaces(10, coordY);
                writer.Write(spaces);
                writer.Write(coordY);

                spaces = CalculateSpaces(10, sforce);
				writer.Write(spaces);
				writer.Write(sforce);

                spaces = CalculateSpaces(08, degree);
				writer.Write(spaces);
				writer.WriteLine(degree);

				//spaces = CalculateSpaces(5, "0.");
				//writer.Write(spaces);
				//writer.Write("0.");

				//spaces = CalculateSpaces(5, "0.");
				//writer.Write(spaces);
				//writer.WriteLine("0.");
			}
        }

        private void WriteSystemEvaluations(BisarSystem bsystem, StreamWriter writer)
        {
            writer.WriteLine(string.Format("  {0}", bsystem.Evaluations.Count));

            string coordX = string.Empty;
            string coordY = string.Empty;
            string coordZ = string.Empty;
            string spaces = string.Empty;

            for (int i = 0; i < bsystem.Evaluations.Count; i++)
            {
                PointResult point = bsystem.Evaluations[i];

                writer.Write(string.Format("  {0}", point.LayerNumber));

                coordX = ConvertToImperial(point.X, 39.370, 2);
                coordY = ConvertToImperial(point.Y, 39.370, 2);
                coordZ = ConvertToImperial(point.Z, 39.370, 2);

                spaces = CalculateSpaces(9, coordX);
                writer.Write(spaces);
                writer.Write(coordX);

                spaces = CalculateSpaces(8, coordY);
                writer.Write(spaces);
                writer.Write(coordY);

                spaces = CalculateSpaces(10, coordZ);
                writer.Write(spaces);
                writer.Write(coordZ);

                spaces = CalculateSpaces(8, "0.");
                writer.Write(spaces);
                writer.WriteLine("0.");
            }
        }

        private string CalculateSpaces(int max, string value)
        {
            string spaces = string.Empty;

            if (!string.IsNullOrEmpty(value))
            {
                for (int i = 0; i < max - value.Length; i++)
                {
                    spaces += " ";
                }
            }

            return spaces;
        }

        private string ConvertToImperial(double number, double ratio, int digits)
        {
            string text = (Math.Round(ratio * number, digits)).ToString("0.00");
            return text.Contains(",") ? (text.Replace(',', '.')) : (text + '.');
        }

        #endregion
    }
}
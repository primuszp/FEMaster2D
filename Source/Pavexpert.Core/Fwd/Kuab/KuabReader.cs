using System;
using System.IO;
using System.Linq;
using Pavexpert.Core.Extensions;
using UnitsNet;

namespace Pavexpert.Core.Fwd.Kuab
{
    public class KuabReader
    {
        #region Members

        private KuabFwd fwd;

        #endregion

        #region Constructors

        public KuabReader()
        {
            fwd = new KuabFwd();
        }

        #endregion

        public IFwdVehicle Read(Stream stream)
        {
            fwd = new KuabFwd();

            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                string[] mtype = null;
                string[] munit = null;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    ReadHeader(line);
                    ReadInstallation(line);
                    ReadJumps(line, ref mtype, ref munit, ref lineNumber);
                    ReadDrops(line);
                    ReadBlock(line);
                }
            }

            return RefillFwdTest();
        }

        private IFwdVehicle RefillFwdTest()
        {
            IFwdVehicle test = new FwdVehicle();

            foreach (KuabDrop kd in fwd.Drops)
            {
                FwdDrop drop = new FwdDrop();

                drop.PeakForce = RefillForce("load", kd.AppliedLoad);
                drop.AirTemperature = RefillTemperature("air", kd.AirTemp);
                drop.PavementTemperature = RefillTemperature("pave", kd.PavementTemp);
                drop.Time = TimeSpan.Parse(kd.Time);
                
                for (int i = 0; i < kd.Deflections.Count; i++)
                {
                    drop.Deflections.Add(RefillLength(string.Format("D{0}", i), kd.Deflections[i]));
                }

            }

            return test;
        }

        private Force RefillForce(string key, double value)
        {
            Force force = new Force();

            var query = fwd.Jumps.FirstOrDefault(x => x.Key.ToLower() == key);

            if (query != null)
            {
                string unit = query.Value;

                switch (unit)
                {
                    case "kgf":
                        force = Force.FromKilogramsForce(value);
                        break;
                    case "lbf":
                        force = Force.FromPoundsForce(value);
                        break;
                }
            }

            return force;
        }

        private Length RefillLength(string key, double value)
        {
            Length length = new Length();

            var query = fwd.Jumps.FirstOrDefault(x => x.Key.ToUpper() == key);

            if (query != null)
            {
                string unit = query.Value;

                switch (unit)
                {
                    case "µm":
                        length = Length.FromMicrometers(value);
                        break;
                    case "mils":
                        length = Length.FromMils(value);
                        break;
                }
            }

            return length;
        }

        private Temperature RefillTemperature(string key, double value)
        {
            Temperature temperature = new Temperature();

            var query = fwd.Jumps.FirstOrDefault(x => x.Key.ToLower() == key);

            if (query != null)
            {
                string unit = query.Value;

                switch (unit)
                {
                    case "°C":
                        temperature = Temperature.FromDegreesCelsius(value);
                        break;
                    case "°F":
                        temperature = Temperature.FromDegreesFahrenheit(value);
                        break;
                }
            }

            return temperature;
        }

        #region Private Methods

        private bool ReadHeader(string buffer)
        {
            if ((buffer.IndexOf("H", StringComparison.Ordinal) == 0) && (buffer.Length >= 20))
            {
                string description = buffer.Substring(1, 17).Trim();
                string content = buffer.Substring(20).Trim();

                fwd.Headers.Add(new KuabHeader(description, content));

                return true;
            }
            return false;
        }

        private bool ReadBlock(string buffer)
        {
            if ((buffer.IndexOf("B", StringComparison.Ordinal) == 0) && (buffer.Length >= 20))
            {
                string description = buffer.Substring(1, 17).Trim();
                string content = buffer.Substring(20).Trim();

                double distance = 0;

                if (fwd.Drops.Count > 0)
                {
                    distance = fwd.Drops[fwd.Drops.Count - 1].Station;
                }

                fwd.Blocks.Add(new KuabBlock(description, content) { Station = distance });

                return true;
            }
            return false;
        }

        private bool ReadInstallation(string buffer)
        {
            if ((buffer.IndexOf("I", StringComparison.Ordinal) == 0) && (buffer.Length >= 20))
            {
                string description = buffer.Substring(1, 17).Trim();
                string content = buffer.Substring(20).Trim();

                fwd.Installations.Add(new KuabInstallation(description, content));

                return true;
            }
            return false;
        }

        private bool ReadDrops(string buffer)
        {
            if (buffer.IndexOf("D", StringComparison.Ordinal) == 0)
            {
                string[] split = buffer.Split(new[] { 'D', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 8) return false;

                KuabDrop drop = new KuabDrop
                {
                    //A mérési szelvény feldolgozása
                    Station = split[0].ToDouble(),

                    //Az ejtések sorrendjének feldolgozása
                    DropNumber = split[1].ToInt(),

                    //Az ejtősúly feldolgozása
                    AppliedLoad = split[2].ToDouble(),

                    //A levegő hőmérséklet feldolgozása
                    AirTemp = split[10].ToDouble(),

                    //A pályaszerkezet hőmérsékletének feldolgozása
                    PavementTemp = split[11].ToDouble(),

                    //Megjegyzés
                    Comment = string.Empty
                };

                //Az ejtések feldolgozása
                int sn = GetSensorCount();

                if (sn > 0)
                {
                    for (int i = 0; i < sn; i++)
                    {
                        drop.Deflections.Add(split[3 + i].ToDouble());
                    }
                }

                //A felületi modulus feldolgozása
                int index = IndexOfKey("Emod", "MPa", "Mpa");

                if (index != -1)
                {
                    drop.EModulus = split[index].ToDouble();
                }

                //A mérési időpont feldolgozása
                index = IndexOfKey("Time", "time", "Idő", "idõ", "ido");

                if (index != -1)
                {
                    drop.Time = split[index];
                }

                fwd.Drops.Add(drop);

                return true;
            }

            if (buffer.Length > 0 && (buffer[0] == 'c' || buffer[0] == 'C'))
            {
                if (fwd.Drops.Count > 0)
                {
                    fwd.Drops[fwd.Drops.Count - 1].Comment = buffer.TrimStart('c', 'C').TrimEnd(' ');

                    if (fwd.Drops[fwd.Drops.Count - 1].Comment.Length > 128)
                    {
                        fwd.Drops[fwd.Drops.Count - 1].Comment = fwd.Drops[fwd.Drops.Count - 1].Comment.Substring(0, 128);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool ReadJumps(string buffer, ref string[] mtype, ref string[] munit, ref int lineNumber)
        {
            if (buffer.IndexOf("J", StringComparison.Ordinal) == 0)
            {
                buffer = buffer.Replace("E Mod", "Emod");
                buffer = buffer.Replace("E mod", "Emod");

                string[] split = buffer.Split(new[] { 'J', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 8) return false;

                switch (lineNumber++)
                {
                    case 0:
                        mtype = split;
                        break;
                    case 1:
                        munit = split;
                        break;
                    default:
                        return false;
                }

                if ((mtype != null) && (munit != null))
                {
                    for (int i = 0; i < 14; i++)
                    {
                        string type = string.Empty;
                        string unit = string.Empty;

                        if (i < mtype.Length) type = mtype[i];
                        if (i < munit.Length) unit = munit[i];

                        fwd.Jumps.Add(new KuabJump(type, unit));
                    }
                    return true;
                }
            }
            return false;
        }

        private int GetSensorCount()
        {
            int count = 0;

            if (fwd.Jumps != null)
            {
                for (int i = 3; i < fwd.Jumps.Count; i++)
                {
                    if (fwd.Jumps[i].Key.Contains("D")) count++;
                }
            }
            return count;
        }

        private int IndexOfKey(params string[] keys)
        {
            for (int i = 0; i < fwd.Jumps.Count; i++)
            {
                if (keys.Any(key => key == fwd.Jumps[i].Key))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
    }
}
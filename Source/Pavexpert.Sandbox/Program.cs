using System;
using System.IO;
using Pavexpert.Core.Fwd.Kuab;
using Pavexpert.Core.OpenPave;

namespace Pavexpert.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            TestOpenPave();
        }




        private static void TestOpenPave()
        {
            int fg = 1;
            int nLayers = 2;

            double[] lh = { 100, 0.0 };
            double[] le = { 300, 50d };
            double[] lv = { 0.3, 0.3 };
            double[] lf = { 1.0, 1.0 };

            int nLoads = 1;

            double[] ax = { 0.0 };
            double[] ay = { 0.0 };
            double[] al = { 50d };
            double[] ar = { 150 };
            double[] ap = { 710 };

            int nPoints = 2;

            double[] px = { 0.0, 100 };
            double[] py = { 0.0, 0.0 };
            double[] pz = { 0.0, 0.0 };
            int[] pl = { 0 };

            double[,] result2 = new double[nPoints, 27];


            var code = OpenPave.Calc(fg, nLayers, lh, le, lv, lf, nLoads, ax, ay, al, ap, ar, nPoints, px, py, pz, pl, result2);
            ;
        }





        private void LoadKuabFwd()
        {
            KuabReader reader = new KuabReader();
            reader.Read(File.OpenRead(@"Y:\Pavement\TU-Továbbfejlesztés\Mérések\Ckt-FWD\813 út ckt jobb oldal.fwd"));

            //Stream output = File.OpenWrite(@"C:\Export\convert.fwd");
            //KuabHelper.ConvertUnitsTo(File.OpenRead(@"C:\Export\813 út ckt jobb oldal.fwd"), output);
        }
    }
}

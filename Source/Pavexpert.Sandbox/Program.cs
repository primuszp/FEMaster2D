using System;
using System.IO;
using Pavexpert.Core.Fwd.Kuab;

namespace Pavexpert.Sandbox
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private void LoadKuabFwd()
        {
            KuabReader reader = new KuabReader();
            reader.Read(File.OpenRead(@"Y:\Pavement\TU-Továbbfejlesztés\Mérések\Ckt-FWD\813 út ckt jobb oldal.fwd"));
            ;

            //Stream output = File.OpenWrite(@"C:\Export\convert.fwd");

            //KuabHelper.ConvertUnitsTo(File.OpenRead(@"C:\Export\813 út ckt jobb oldal.fwd"), output);
        }
    }
}

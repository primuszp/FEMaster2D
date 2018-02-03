using System;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace Pavexpert.Core.OpenPave
{
    /// <summary>
    /// DLL wrapper for OpenPave classes
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static partial class OpenPave
    {
        /// <summary>
        /// Layered elastic calculation with circular loads
        /// </summary>
        /// <param name="flags">Flags to choose method</param>
        /// <param name="nl">Number of layers</param>
        /// <param name="lt">Layer thickness (0 for semi-inf)</param>
        /// <param name="em">Elastic modulus</param>
        /// <param name="pr">Poisson's ratio</param>
        /// <param name="lf">Layer friction (0.0 to 1.0)</param>
        /// <param name="na">Number of loads</param>
        /// <param name="ax">Center X location</param>
        /// <param name="ay">Center Y location</param>
        /// <param name="al">Load (0 for auto)</param>
        /// <param name="ap">Pressure (0 for auto)</param>
        /// <param name="ar">Radius (0 for auto)</param>
        /// <param name="np">Number of evaluation points</param>
        /// <param name="px">Point X</param>
        /// <param name="py">Point Y</param>
        /// <param name="pz">Point Z</param>
        /// <param name="pl">Point layer (0 for auto)</param>
        /// <param name="results">Results</param>
        public static int Calculation(int flags, int nl, double[] lt, double[] em, double[] pr, double[] lf,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] results)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                if (Environment.Is64BitProcess)
                {
                    return OPLECalcX64(flags, nl, lt, em, pr, lf, na, ax, ay, al, ap, ar, np, px, py, pz, pl, results);
                }

                return OPLECalcX86(flags, nl, lt, em, pr, lf, na, ax, ay, al, ap, ar, np, px, py, pz, pl, results);
            }

            return OPLECalcOSX(flags, nl, lt, em, pr, lf, na, ax, ay, al, ap, ar, np, px, py, pz, pl, results);
        }
    }
}
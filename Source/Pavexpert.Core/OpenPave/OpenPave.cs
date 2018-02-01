using System;
using System.Security.Permissions;

namespace Pavexpert.Core.OpenPave
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static partial class OpenPave
    {
        public static int Calc(int flags, int nl, double[] h, double[] E, double[] v, double[] f,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] res)
        {
            if (Environment.Is64BitProcess)
            {
                return OPLECalc64(flags, nl, h, E, v, f, na, ax, ay, al, ap, ar, np, px, py, pz, pl, res);
            }

            return OPLECalc86(flags, nl, h, E, v, f, na, ax, ay, al, ap, ar, np, px, py, pz, pl, res);
        }
    }
}
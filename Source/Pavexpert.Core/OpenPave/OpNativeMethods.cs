using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace Pavexpert.Core.OpenPave
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static partial class OpenPave
    {
        private const string NativeLibrary86 = "Libraries\\libop.dll";
        private const string NativeLibrary64 = "Libraries\\libop64.dll";
        private const CallingConvention Convention = CallingConvention.Cdecl;

        #region Native Functions

        [DllImport(NativeLibrary86, EntryPoint = "_OP_LE_Calc@72", CallingConvention = Convention), SuppressUnmanagedCodeSecurity]
        private static extern int OPLECalc86(int flags, int nl, double[] h, double[] E, double[] v, double[] f,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] res);

        [DllImport(NativeLibrary64, EntryPoint = "OP_LE_Calc", CallingConvention = Convention), SuppressUnmanagedCodeSecurity]
        private static extern int OPLECalc64(int flags, int nl, double[] h, double[] E, double[] v, double[] f,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] res);

        #endregion
    }
}
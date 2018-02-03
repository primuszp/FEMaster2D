using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace Pavexpert.Core.OpenPave
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static partial class OpenPave
    {
        private const string NativeLibraryX86 = "Libraries\\libop.dll";
        private const string NativeLibraryX64 = "Libraries\\libop64.dll";
        private const string NativeLibraryOSX = "Libraries\\libop.dylib";
        private const CallingConvention Convention = CallingConvention.Cdecl;

        #region Native Functions


        [DllImport(NativeLibraryX86, EntryPoint = "_OP_LE_Calc@72", CallingConvention = Convention), SuppressUnmanagedCodeSecurity]
        private static extern int OPLECalcX86(int flags, int nl, double[] lt, double[] em, double[] pr, double[] lf,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] results);

        [DllImport(NativeLibraryX64, EntryPoint = "OP_LE_Calc", CallingConvention = Convention), SuppressUnmanagedCodeSecurity]
        private static extern int OPLECalcX64(int flags, int nl, double[] lt, double[] em, double[] pr, double[] lf,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] results);

        [DllImport(NativeLibraryOSX, EntryPoint = "OP_LE_Calc", CallingConvention = Convention), SuppressUnmanagedCodeSecurity]
        private static extern int OPLECalcOSX(int flags, int nl, double[] lt, double[] em, double[] pr, double[] lf,
            int na, double[] ax, double[] ay, double[] al, double[] ap, double[] ar,
            int np, double[] px, double[] py, double[] pz, int[] pl,
            double[,] results);

        #endregion
    }
}
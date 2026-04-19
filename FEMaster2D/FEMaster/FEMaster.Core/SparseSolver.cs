using System;
using System.Collections.Generic;

namespace FEMaster.Core
{
    // CSR symmetric sparse matrix (stores full rows for simplicity; upper+lower).
    // Assembly happens via a temporary Dictionary then converts to CSR arrays.
    internal class SparseMatrix
    {
        private readonly int n;
        private int[]    rowPtr;
        private int[]    colIdx;
        private double[] values;

        public int Size => n;

        private SparseMatrix(int n, int[] rowPtr, int[] colIdx, double[] values)
        {
            this.n      = n;
            this.rowPtr = rowPtr;
            this.colIdx = colIdx;
            this.values = values;
        }

        // Returns diagonal entries (used for Jacobi preconditioning).
        public double[] Diagonal()
        {
            double[] d = new double[n];
            for (int i = 0; i < n; i++)
                for (int k = rowPtr[i]; k < rowPtr[i + 1]; k++)
                    if (colIdx[k] == i)
                        d[i] = values[k];
            return d;
        }

        // y = A * x
        public void Multiply(double[] x, double[] y)
        {
            for (int i = 0; i < n; i++)
            {
                double s = 0;
                for (int k = rowPtr[i]; k < rowPtr[i + 1]; k++)
                    s += values[k] * x[colIdx[k]];
                y[i] = s;
            }
        }

        // Builder: accepts (row, col, value) triples, col >= row or col < row both OK.
        internal class Builder
        {
            private readonly int n;
            // upper triangle only, key = row*n+col (row<=col)
            private readonly Dictionary<long, double> entries;

            public Builder(int n)
            {
                this.n   = n;
                entries  = new Dictionary<long, double>(n * 6);
            }

            public void Add(int row, int col, double val)
            {
                if (row > col) { int t = row; row = col; col = t; } // keep upper triangle
                long key = (long)row * n + col;
                if (entries.TryGetValue(key, out double existing))
                    entries[key] = existing + val;
                else
                    entries[key] = val;
            }

            public SparseMatrix Build()
            {
                // Count entries per row (symmetric: each off-diag counted twice)
                int[] count = new int[n];
                foreach (var kv in entries)
                {
                    int row = (int)(kv.Key / n);
                    int col = (int)(kv.Key % n);
                    count[row]++;
                    if (row != col) count[col]++;
                }

                int[] rowPtr = new int[n + 1];
                for (int i = 0; i < n; i++) rowPtr[i + 1] = rowPtr[i] + count[i];

                int nnz = rowPtr[n];
                int[] colIdx = new int[nnz];
                double[] vals = new double[nnz];
                int[] pos = (int[])rowPtr.Clone(); // current fill pointer per row

                foreach (var kv in entries)
                {
                    int row = (int)(kv.Key / n);
                    int col = (int)(kv.Key % n);
                    double v = kv.Value;

                    colIdx[pos[row]] = col; vals[pos[row]++] = v;
                    if (row != col) { colIdx[pos[col]] = row; vals[pos[col]++] = v; }
                }

                return new SparseMatrix(n, rowPtr, colIdx, vals);
            }
        }
    }

    // Preconditioned Conjugate Gradient for symmetric positive definite A.
    // Uses diagonal (Jacobi) preconditioner.
    internal static class PCG
    {
        public static double[] Solve(SparseMatrix A, double[] b, double tol = 1e-10, int maxIter = -1)
        {
            int n = A.Size;
            if (maxIter < 0) maxIter = Math.Max(2 * n, 5000);

            double[] diag = A.Diagonal();
            // Jacobi preconditioner: M^-1 = 1/diag(A)
            double[] minvDiag = new double[n];
            for (int i = 0; i < n; i++)
                minvDiag[i] = diag[i] > 0 ? 1.0 / diag[i] : 1.0;

            double[] x  = new double[n];
            double[] r  = (double[])b.Clone();  // r = b - A*x0,  x0=0
            double[] z  = ApplyDiagPrecon(minvDiag, r);
            double[] p  = (double[])z.Clone();
            double   rz = Dot(r, z);
            double[] Ap = new double[n];

            double bNorm = Math.Sqrt(Dot(b, b));
            if (bNorm < double.Epsilon) return x;

            for (int iter = 0; iter < maxIter; iter++)
            {
                A.Multiply(p, Ap);
                double pAp   = Dot(p, Ap);
                if (Math.Abs(pAp) < double.Epsilon) break;
                double alpha = rz / pAp;

                for (int i = 0; i < n; i++) { x[i] += alpha * p[i]; r[i] -= alpha * Ap[i]; }

                double residNorm = Math.Sqrt(Dot(r, r));
                if (residNorm / bNorm < tol) break;

                z = ApplyDiagPrecon(minvDiag, r);
                double rzNew = Dot(r, z);
                double beta  = rzNew / rz;
                rz = rzNew;
                for (int i = 0; i < n; i++) p[i] = z[i] + beta * p[i];
            }
            return x;
        }

        private static double[] ApplyDiagPrecon(double[] minv, double[] v)
        {
            double[] z = new double[v.Length];
            for (int i = 0; i < v.Length; i++) z[i] = minv[i] * v[i];
            return z;
        }

        private static double Dot(double[] a, double[] b)
        {
            double s = 0;
            for (int i = 0; i < a.Length; i++) s += a[i] * b[i];
            return s;
        }
    }
}

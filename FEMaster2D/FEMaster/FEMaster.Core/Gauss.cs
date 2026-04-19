using System;
using System.Threading;
using System.Threading.Tasks;

namespace FEMaster.Core
{
    // Gauss elimination for symmetric positive-definite banded matrices.
    // Storage: a[i, j] = A[i, i+j] for j = 0..halfBandWidth-1 (upper triangle + diagonal).
    // The parallel forward elimination is safe because each row index is unique within
    // the Parallel.For body — threads read from the shared (read-only) pivot row and
    // write only to their own row. Thread.VolatileRead is used on x[pRow] to guarantee
    // visibility across threads.
    public class Gauss
    {
        private const int ParallelRowThreshold = 12;

        private readonly int n;
        private readonly int hbw;
        private readonly double[,] a;

        public Gauss(double[,] bandedMatrix)
        {
            n   = bandedMatrix.GetLength(0);
            hbw = bandedMatrix.GetLength(1);
            a   = bandedMatrix;
        }

        // Serial solve – modifies x in place (caller passes a copy).
        public void SolveSerial(ref double[] x)
        {
            ForwardElimSerial(x);
            BackSubstitute(x);
        }

        // Parallel solve – returns solution, does not modify rhs.
        public double[] Solve(double[] rhs)
        {
            var x = new double[rhs.Length];
            Array.Copy(rhs, x, rhs.Length);
            ForwardElimParallel(x);
            BackSubstitute(x);
            return x;
        }

        private void ForwardElimSerial(double[] x)
        {
            for (int pRow = 0; pRow < n - 1; pRow++)
            {
                double pivot = a[pRow, 0];
                int rMax = Math.Min(pRow + hbw, n);

                for (int row = pRow + 1; row < rMax; row++)
                {
                    double aik = a[pRow, row - pRow] / pivot;
                    int colMax = hbw - (row - pRow);
                    for (int col = 0; col < colMax; col++)
                        a[row, col] -= aik * a[pRow, row - pRow + col];
                    x[row] -= aik * x[pRow];
                }
            }
        }

        private void ForwardElimParallel(double[] x)
        {
            for (int pRow = 0; pRow < n - 1; pRow++)
            {
                double pivot = a[pRow, 0];
                int rMax = Math.Min(pRow + hbw, n);
                int rowCount = rMax - pRow - 1;

                // For narrow bands the thread overhead exceeds the gain.
                if (rowCount < ParallelRowThreshold)
                {
                    // inline serial path — same as ForwardElimSerial
                    double xp = x[pRow];
                    for (int row = pRow + 1; row < rMax; row++)
                    {
                        double aik = a[pRow, row - pRow] / pivot;
                        int colMax = hbw - (row - pRow);
                        for (int col = 0; col < colMax; col++)
                            a[row, col] -= aik * a[pRow, row - pRow + col];
                        x[row] -= aik * xp;
                    }
                }
                else
                {
                    // Each parallel task owns a unique row index.
                    // a[pRow, ...] is read-only in this phase.
                    // x[pRow] is read-only here — captured via local for clarity.
                    double xp = Thread.VolatileRead(ref x[pRow]);
                    int pRowCopy = pRow;

                    Parallel.For(pRow + 1, rMax, row =>
                    {
                        double aik = a[pRowCopy, row - pRowCopy] / pivot;
                        int colMax = hbw - (row - pRowCopy);
                        for (int col = 0; col < colMax; col++)
                            a[row, col] -= aik * a[pRowCopy, row - pRowCopy + col];
                        x[row] -= aik * xp;
                    });
                }
            }
        }

        private void BackSubstitute(double[] x)
        {
            x[n - 1] /= a[n - 1, 0];

            for (int k = n - 2; k >= 0; k--)
            {
                double sum = 0.0;
                int uBound = Math.Min(hbw - 1, n - k - 1);
                for (int j = 1; j <= uBound; j++)
                    sum += a[k, j] * x[k + j];
                x[k] = (x[k] - sum) / a[k, 0];
            }
        }
    }
}

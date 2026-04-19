using System;

namespace FEMaster.Core
{
    /// <summary>
    /// Implementation of the Constant Strain Triangle (CST)
    /// </summary>
    public class CsTriangle
    {
        #region Members

        private readonly double x1, y1, x2, y2, x3, y3;
        private readonly double x13, x32, x21, x23, y13, y23, y31, y12;
        private readonly ProblemTypes ProblemType = ProblemTypes.PlaneStress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the elastic modulus.
        /// </summary>
        /// <value>
        /// The elastic modulus in [Pa] dimension.
        /// </value>
        public double ElasticModulus { get; set; }

        /// <summary>
        /// Gets or sets the Poisson ratio.
        /// </summary>
        /// <value>
        /// The Poisson ratio.
        /// </value>
        public double PoissonRatio { get; set; }

        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>
        /// The thickness of this member, in [m] dimension.
        /// </value>
        public double Thickness { get; set; }

        /// <summary>
        /// Gets the area of triangular element.
        /// </summary>
        public double Area => GetDetofJacobian() * 0.5;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of Constant Strain Triangle (CST) element
        /// </summary>
        public CsTriangle(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3,
            double thickness, double modulus, double poisson)
        {
            this.x1 = x1; this.y1 = y1;
            this.x2 = x2; this.y2 = y2;
            this.x3 = x3; this.y3 = y3;

            Thickness = thickness;
            PoissonRatio = poisson;
            ElasticModulus = modulus;

            // Calculate the distances. They will be required for computing the Jacobian and B matrices.

            x13 = x1 - x3;
            x32 = x3 - x2;
            x21 = x2 - x1;
            x23 = x2 - x3;
            y13 = y1 - y3;
            y23 = y2 - y3;
            y31 = y3 - y1;
            y12 = y1 - y2;
        }

        #endregion

        /// <summary>
        /// Returns the [B] matrix.
        /// </summary>
        public double[,] ComputeBMatrix()
        {
            var detJ = GetDetofJacobian();
            var matB = new double[3, 6];

            matB[0, 0] = y23 / detJ;
            matB[1, 0] = 0.0;
            matB[2, 0] = x32 / detJ;

            matB[0, 1] = 0.0;
            matB[1, 1] = x32 / detJ;
            matB[2, 1] = y23 / detJ;

            matB[0, 2] = y31 / detJ;
            matB[1, 2] = 0.0;
            matB[2, 2] = x13 / detJ;

            matB[0, 3] = 0.0;
            matB[1, 3] = x13 / detJ;
            matB[2, 3] = y31 / detJ;

            matB[0, 4] = y12 / detJ;
            matB[1, 4] = 0.0;
            matB[2, 4] = x21 / detJ;

            matB[0, 5] = 0.0;
            matB[1, 5] = x21 / detJ;
            matB[2, 5] = y12 / detJ;

            return matB;
        }

        /// <summary>
        /// Returns the [D] matrix.
        /// </summary>
        /// <returns></returns>
        public double[,] ComputeDMatrix()
        {
            var matD = new double[3, 3];

            switch (ProblemType)
            {
                case ProblemTypes.PlaneStress:
                    {
                        var cf = ElasticModulus / (1.0 - PoissonRatio * PoissonRatio);

                        matD[0, 0] = 1.0 * cf;
                        matD[1, 0] = PoissonRatio * cf;
                        matD[2, 0] = 0.0;

                        matD[0, 1] = PoissonRatio * cf;
                        matD[1, 1] = 1.0 * cf;
                        matD[2, 1] = 0.0;

                        matD[0, 2] = 0.0;
                        matD[1, 2] = 0.0;
                        matD[2, 2] = (1.0 - PoissonRatio) * 0.5 * cf;
                        break;
                    }
                case ProblemTypes.PlaneStrain:
                    {
                        var cf = ElasticModulus / ((1.0 + PoissonRatio) * (1.0 - 2 * PoissonRatio));

                        matD[0, 0] = (1.0 - PoissonRatio) * cf;
                        matD[1, 0] = PoissonRatio * cf;
                        matD[2, 0] = 0.0;

                        matD[0, 1] = PoissonRatio * cf;
                        matD[1, 1] = (1.0 - PoissonRatio) * cf;
                        matD[2, 1] = 0.0;

                        matD[0, 2] = 0.0;
                        matD[1, 2] = 0.0;
                        matD[2, 2] = (0.5 - PoissonRatio) * cf;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return matD;
        }

        /// <summary>
        /// Gets the stiffness matrix [Ke] of CST element.
        /// </summary>
        /// <returns></returns>
        public double[,] ComputeStiffnessMatrix()
        {
            double[,] d = ComputeDMatrix();
            double[,] b = ComputeBMatrix();

            double[,] bT = ComputeTranspose(ref b);

            // Compute [Ke] matrix now

            double[,] bTd = MultiplyMatrices(ref bT, ref d);
            double[,] mKe = MultiplyMatrices(ref bTd, ref b);

            // Multiply [Ke] matrix by thickness * area

            return MultiplyByConstant(ref mKe, Thickness * Area);
        }

        /// <summary>
        /// Returns the determinant of the Jacobian matrix.
        /// </summary>
        /// <returns></returns>
        private double GetDetofJacobian()
        {
            return x13 * y23 - x23 * y13;
        }

        /// <summary>
        /// Swaps each matrix entry A[i, j] with A[j, i].
        /// </summary>
        /// <returns>A transposed matrix.</returns>
        private double[,] ComputeTranspose(ref double[,] matrix)
        {
            var mi = matrix.GetLength(0);
            var mj = matrix.GetLength(1);

            var matrixT = new double[mj, mi];

            for (var i = 0; i < mi; i++)
            {
                for (var j = 0; j < mj; j++)
                {
                    matrixT[j, i] = matrix[i, j];
                }
            }

            return matrixT;
        }

        /// <summary>
        /// Multiplies matrix [A] by [B] and returns the product.
        /// </summary>
        private double[,] MultiplyMatrices(ref double[,] m1, ref double[,] m2)
        {
            var m1Rows = m1.GetLength(0);
            var m1Cols = m1.GetLength(1);

            var m2Rows = m2.GetLength(0);
            var m2Cols = m2.GetLength(1);

            if (m1Cols != m2Rows)
                throw new InvalidOperationException("No consistent dimensions");

            var matrix = new double[m1Rows, m2Cols];

            for (var i = 0; i < m1Rows; i++)
            {
                for (var j = 0; j < m2Cols; j++)
                {
                    for (var k = 0; k < m1Cols; k++)
                    {
                        matrix[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }

            return matrix;
        }

        private double[,] MultiplyByConstant(ref double[,] matrix, double constant)
        {
            var mi = matrix.GetLength(0);
            var mj = matrix.GetLength(1);

            for (var i = 0; i < mi; i++)
            {
                for (var j = 0; j < mj; j++)
                {
                    matrix[i, j] = matrix[i, j] * constant;
                }
            }

            return matrix;
        }
    }
}
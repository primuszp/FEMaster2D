using System;

namespace ShellBisar.Results
{
    /// <summary>
    /// Evaluation Point
    /// </summary>
    public class PointResult
    {
        #region Properties

        public int Number { get; set; }

        public int LayerNumber { get; set; }

        public ModelResult Stress { get; private set; }

        public ModelResult Strain { get; private set; }

        public ModelResult Displacement { get; private set; }

        /// <summary>
        /// X-Coordinate
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-Coordinate
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Z-Depth
        /// </summary>
        public double Z { get; set; }

        public bool IsStandardLocation
        {
            get
            {
                return X.Equals(0) && Y.Equals(0);
            }
            set
            {
                if (value == true)
                {
                    X = 0;
                    Y = 0;
                }
            }
        }

        #endregion

        #region Constructors

        public PointResult()
            : this(0, 0, 0, 0, 0)
        { }

        public PointResult(int number, int layerNumber, double z)
            : this(number, layerNumber, 0, 0, z)
        { }

        public PointResult(int number, int layerNumber, double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;

            Number = number;
            LayerNumber = layerNumber;

            Stress = ModelResult.Stress;
            Strain = ModelResult.Strain;
            Displacement = ModelResult.Displacement;
        }

        #endregion

        public void SetStress(double xx, double yy, double zz)
        {
            SetStress(xx, yy, zz, 0.0d, 0.0d, 0.0d);
        }

        public void SetStress(double xx, double yy, double zz, double yz, double xz, double xy)
        {
            Stress = new ModelResult(xx, yy, zz, yz, xz, xy, ModelResultType.Stress);
        }

        public void SetStrain(double xx, double yy, double zz)
        {
            SetStrain(xx, yy, zz, 0.0d, 0.0d, 0.0d);
        }

        public void SetStrain(double xx, double yy, double zz, double yz, double xz, double xy)
        {
            Strain = new ModelResult(xx, yy, zz, yz, xz, xy, ModelResultType.Strain);
        }

        public void SetDisplacement(double xx, double yy, double zz)
        {
            SetDisplacement(xx, yy, zz, 0.0d, 0.0d, 0.0d);
        }

        public void SetDisplacement(double xx, double yy, double zz, double yz, double xz, double xy)
        {
            Displacement = new ModelResult(xx, yy, zz, yz, xz, xy, ModelResultType.Displacement);
        }
    }
}
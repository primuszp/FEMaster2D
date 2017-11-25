using System;

namespace Pavexpert.Core.Results
{
    public class ModelResult
    {
        #region Properties

        public double XX { get; set; }

        public double YY { get; set; }

        public double ZZ { get; set; }

        public double ShearYZ { get; set; }

        public double ShearXZ { get; set; }

        public double ShearXY { get; set; }

        public ModelResultType ResultType { get; set; }

        #endregion

        #region Constructors

        public ModelResult()
            : this(0d, 0d, 0d, 0d, 0d, 0d)
        { }

        public ModelResult(double xx, double yy, double zz)
            : this(xx, yy, zz, 0d, 0d, 0d)
        { }

        public ModelResult(double xx, double yy, double zz,
                           double yz, double xz, double xy,
                           ModelResultType resultType = ModelResultType.Stress)
        {
            XX = xx;
            YY = yy;
            ZZ = zz;
            ShearYZ = yz;
            ShearXZ = xz;
            ShearXY = xy;
            ResultType = resultType;
        }

        #endregion

        #region Static Constants

        public static ModelResult Stress = new ModelResult { ResultType = ModelResultType.Stress };
        public static ModelResult Strain = new ModelResult { ResultType = ModelResultType.Strain };
        public static ModelResult Displacement = new ModelResult { ResultType = ModelResultType.Displacement };

        #endregion
    }
}
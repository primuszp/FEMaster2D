using System.Collections.Generic;

namespace Pavexpert.Core.Fwd.Kuab
{
    public class KuabDrop
    {
        #region Properties

        public int DropNumber { get; set; }

        public double Station { get; set; }

        public double AppliedLoad { get; set; }

        public double AirTemp { get; set; }

        public double PavementTemp { get; set; }

        public double EModulus { get; set; }

        public string Comment { get; set; }

        public string Time { get; set; }

        public IList<double> Deflections { get; set; }

        #endregion

        #region Construction

        public KuabDrop()
        {
            Deflections = new List<double>();
        }

        #endregion
    }
}
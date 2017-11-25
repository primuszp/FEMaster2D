using Pavexpert.Core.Results;
using Pavexpert.Core.Pavement;
using System.Collections.Generic;

namespace Pavexpert.Core.Bisar
{
    public class BisarSystem
    {
        #region Properties

        public int Number { get; set; }

        public List<Load> Loads { get; set; }

        public List<Layer> Layers { get; set; }

        public List<PointResult> Evaluations { get; set; }

		public SpringComplianceType SpringCompliance { get; set; }

        public CalculationMethodType CalculationMethod { get; set; }

        #endregion

        #region Constructors

        public BisarSystem()
        {
            Loads = new List<Load>();
            Layers = new List<Layer>();
            Evaluations = new List<PointResult>();
			SpringCompliance = SpringComplianceType.Reduced;
            CalculationMethod = CalculationMethodType.Rough;
        }

        #endregion
    }
}
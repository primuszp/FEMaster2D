using System;
using UnitsNet;

namespace Pavexpert.Core.Fwd
{
    public class FwdDrop : IFwdDrop
    {
        public Force PeakForce { get; set; }

        public Temperature AirTemperature { get; set; }

        public Temperature PavementTemperature { get; set; }

        public System.Collections.Generic.IList<Length> Deflections { get; set; }

        public TimeSpan Time { get; set; }

        public FwdDrop()
        {
            PeakForce = Force.Zero;
            AirTemperature = Temperature.Zero;
            PavementTemperature = Temperature.Zero;
            Deflections = new System.Collections.Generic.List<Length>();
            Time = TimeSpan.Zero;
        }
    }
}
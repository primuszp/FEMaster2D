using System;
using System.Collections.Generic;
using UnitsNet;

namespace Pavexpert.Core.Fwd
{
    public interface IFwdDrop
    {
        Force PeakForce { get; set; }

        /// <summary>
        /// Air Temperature
        /// </summary>
        Temperature AirTemperature { get; set; }

        /// <summary>
        /// Pavement Temperature
        /// </summary>
        Temperature PavementTemperature { get; set; }

        IList<Length> Deflections { get; set; }

        TimeSpan Time { get; set; }
    }
}
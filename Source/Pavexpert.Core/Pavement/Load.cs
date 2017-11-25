using System;

namespace Pavexpert.Core.Pavement
{
    public class Load
    {
        #region Properties

        /// <summary>
        /// No of Circular Load
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Load Position X [m]
        /// </summary>
        /// <value>X-Coordinate of Load [m]</value>
        public double X { get; set; }

        /// <summary>
        /// Load Position Y [m]
        /// </summary>
        /// <value>Y-Coordinate of Load [m]</value>
        public double Y { get; set; }

        /// <summary>
        /// Vertical Load [kN]
        /// </summary>
        public double NormalLoad { get; private set; }

        /// <summary>
        /// Vertical Stress [MPa]
        /// </summary>
        public double NormalStress { get; private set; }

        /// <summary>
        /// Radius of Loaded Area [m]
        /// </summary>
        public double Radius { get; private set; }

        /// <summary>
        /// Horizontal Load [kN]
        /// </summary>
        public double ShearLoad { get; private set; }

        /// <summary>
        /// Horizontal Stress [MPa]
        /// </summary>
        public double ShearStress { get; private set; }

        /// <summary>
        /// Shear Angle [degrees]
        /// </summary>
        public double ShearDirection { get; private set; }

        #endregion

        #region Constructors

        public Load() : this(1, 0.0, 0.0)
        { }

        public Load(int n, double xcoord, double ycoord)
        {
            Number = n;
            X = xcoord;
            Y = ycoord;
        }

        public Load(int n, double xcoord, double ycoord,
                    double vload, double radius, double hload = 0.0d, double angle = 0.0d)
        {
            Number = n;
            X = xcoord;
            Y = ycoord;
            SetLoadAndRadius(vload, radius, hload, angle);
        }

        #endregion

        #region Setter Methods

        /// <summary>
        /// Set the load characteristics with stress and load.
        /// </summary>
        /// <param name="vload">Vertical (normal) Load [kN]</param>
        /// <param name="vstress">Vertical (normal) Stress [MPa]</param>
        /// <param name="hstress">Horizontal (shear) Stress [MPa]</param>
        /// <param name="angle">Shear Angle [degrees]</param>
        public void SetStressAndLoad(double vload, double vstress, double hstress = 0.0d, double angle = 0.0d)
        {
            NormalLoad = vload;
            ShearStress = hstress;
            NormalStress = vstress;
            ShearDirection = angle;

            Radius = Math.Round(Math.Sqrt(NormalLoad / (1000 * Math.PI * NormalStress)), 3);
            ShearLoad = Math.Round(ShearStress * (1000.000 * Radius * Radius * Math.PI), 3);
        }

        /// <summary>
        /// Set the load characteristics with load and radius.
        /// </summary>
        /// <param name="vload">Vertical (normal) Load [kN]</param>
        /// <param name="radius">Radius of Loaded Area [m]</param>
        /// <param name="hload">Horizontal (shear) Load [kN]</param>
        /// <param name="angle">Shear Angle [degrees]</param>
        public void SetLoadAndRadius(double vload, double radius = 0.150d, double hload = 0.0d, double angle = 0.0d)
        {
            Radius = radius;
            ShearLoad = hload;
            NormalLoad = vload;
            ShearDirection = angle;

            ShearStress = Math.Round(ShearLoad / (1000.0 * Radius * Radius * Math.PI), 3);
            NormalStress = Math.Round(NormalLoad / (1000 * Radius * Radius * Math.PI), 3);
        }

        /// <summary>
        /// Set the load characteristics with stress and radius.
        /// </summary>
        /// <param name="vstress">Vertical (normal) Stress [MPa]</param>
        /// <param name="radius">Radius of Loaded Area [m]</param>
        /// <param name="hstress">Horizontal (shear) Stress [MPa]</param>
        /// <param name="angle">Shear Angle [degrees]</param>
        public void SetStressAndRadius(double vstress, double radius = 0.150d, double hstress = 0.0d, double angle = 0.0d)
        {
            Radius = radius;
            ShearStress = hstress;
            NormalStress = vstress;
            ShearDirection = angle;

            ShearLoad = Math.Round(ShearStress * (1000.0 * Radius * Radius * Math.PI), 3);
            NormalLoad = Math.Round(NormalStress * (1000 * Radius * Radius * Math.PI), 3);
        }

        #endregion
    }
}
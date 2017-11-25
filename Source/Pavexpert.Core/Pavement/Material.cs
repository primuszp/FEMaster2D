namespace Pavexpert.Core.Pavement
{
    public class Material
    {
		#region Properties

		/// <summary>
        /// Modulus of Elasticity [MPa]
		/// </summary>
		public double Modulus { get; set; }

		/// <summary>
        /// Poisson’s ratio [-]
		/// </summary>
		public double Poisson { get; set; }

        #endregion

        #region Constructors

        public Material(double modulus, double poisson)
        {
            Modulus = modulus;
            Poisson = poisson;
        }

        public Material(double modulus)
            : this(modulus, 0.5)
        { }

        public Material()
        {
            Modulus = 50.0d;
            Poisson = 0.50d;
        }

        #endregion

        #region Static Constants

        public static Material Subgrade = new Material(50, 0.5);

        #endregion
    }
}
namespace Pavexpert.Core.Pavement
{
    public class Layer
    {
        #region Properties

        public int Number { get; set; }

        public double Thickness { get; set; }

		/// <summary>
		/// <para>Interface Friction Parameter (IFP), with 0 ≤ IFP ≤ 1</para>
		/// <para>IFP = 0 means full friction, IFP = 1 means complete slip</para>
		/// </summary>
		public double Roughness { get; set; }

        /// <summary>
        /// Material
        /// </summary>
        public Material Material { get; set; }

        public bool IsHalfSpace
        {
            get { return double.IsPositiveInfinity(Thickness); }
        }

        #endregion

        #region Constructors

        public Layer()
            : this(0, Material.Subgrade)
        { }

        public Layer(int number, Material material,
                     double thickness = double.PositiveInfinity,
                     double roughness = 0.0)
        {
            Number = number;
            Material = material;
            Thickness = thickness;
            Roughness = roughness;
        }

        #endregion
    }
}
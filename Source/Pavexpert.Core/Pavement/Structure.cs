using System.Linq;
using System.Collections.Generic;

namespace Pavexpert.Core.Pavement
{
    public class Structure
    {
		#region Properties

		public List<Layer> Layers { get; set; }

		public StructureType StructureType { get; set; }

		#endregion

		#region Constructors

		public Structure()
		{
			Layers = new List<Layer>();
			StructureType = StructureType.Flexible;
		}

		#endregion

		public Layer GetLayerByNumber(int number)
		{
            return Layers.FirstOrDefault(layer => layer.Number == number);
		}

		public Layer GetHalfSpace()
		{
			return Layers.FirstOrDefault(layer => layer.IsHalfSpace == true);
		}
	}
}
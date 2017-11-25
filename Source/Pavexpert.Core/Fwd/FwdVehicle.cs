namespace Pavexpert.Core.Fwd
{
    public class FwdVehicle : IFwdVehicle
    {
        public System.Collections.Generic.IList<IFwdDrop> Drops { get; set; }

        public FwdVehicle()
        {
            Drops = new System.Collections.Generic.List<IFwdDrop>();
        }
    }
}
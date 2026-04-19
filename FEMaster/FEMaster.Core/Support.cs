namespace FEMaster.Core
{
    public class Support
    {
        public int SupportNo { get; set; }
        public int NodeNo { get; set; }
        public int RestraintX { get; set; } // 1 or 0
        public int RestraintY { get; set; }

        public Support(int sp, int nn, int restraintX, int restraintY)
        {
            SupportNo = sp;
            NodeNo = nn;
            RestraintX = restraintX;
            RestraintY = restraintY;
        }
    }
}
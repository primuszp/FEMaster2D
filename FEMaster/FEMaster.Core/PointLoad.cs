namespace FEMaster.Core
{
    public class PointLoad
    {
        public int LoadNo { get; set; }
        public int NodeNo { get; set; }

        public double Fx { get; set; }
        public double Fy { get; set; }

        public PointLoad(int ln, int nn, double fx, double fy)
        {
            LoadNo = ln;
            NodeNo = nn;
            Fx = fx; Fy = fy;
        }
    }
}
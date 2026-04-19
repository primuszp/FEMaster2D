namespace FEMaster.Core
{
    public class Node
    {
        public int NodeNo { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public Node(int nn, double nx, double ny)
        {
            NodeNo = nn;

            X = nx;
            Y = ny;
        }
    }
}
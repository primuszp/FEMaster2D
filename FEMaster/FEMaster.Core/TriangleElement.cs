namespace FEMaster.Core
{
    public class TriangleElement
    {
        public int ElementNo { get; set; }

        public int Node1 { get; set; }

        public int Node2 { get; set; }

        public int Node3 { get; set; }

        public double[] Strains = new double[3];

        public double[] Stresses = new double[3];

        public TriangleElement(int n, int node1, int node2, int node3)
        {
            ElementNo = n;
            Node1 = node1;
            Node2 = node2;
            Node3 = node3;
        }
    }
}
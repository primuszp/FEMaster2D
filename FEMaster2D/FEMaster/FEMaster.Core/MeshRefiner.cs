using System.Collections.Generic;

namespace FEMaster.Core
{
    // Uniform 1-to-4 triangle refinement by edge midpoint splitting.
    //
    //        n3
    //       /  \
    //     m13 - m23
    //     / \  / \
    //    n1 - m12 - n2
    //
    // Each original triangle becomes 4 child triangles.
    // Shared edges produce only one midpoint node (dictionary lookup).
    public static class MeshRefiner
    {
        public static Model Refine(Model source)
        {
            var edgeMid = new Dictionary<long, int>(); // edge key → new node index (1-based)
            var nodes   = new List<Node>(source.NodesNo * 2);
            var elems   = new List<TriangleElement>(source.ElementsNo * 4);

            // Copy original nodes
            for (int i = 0; i < source.NodesNo; i++)
                nodes.Add(new Node(source.Nodes[i].NodeNo,
                                   source.Nodes[i].X,
                                   source.Nodes[i].Y));

            int nextNodeNo = source.NodesNo + 1;
            int nextElemNo = 1;

            for (int e = 0; e < source.ElementsNo; e++)
            {
                int a = source.Elements[e].Node1;
                int b = source.Elements[e].Node2;
                int c = source.Elements[e].Node3;

                int mab = GetOrCreateMidpoint(a, b, ref nextNodeNo, nodes, source, edgeMid);
                int mbc = GetOrCreateMidpoint(b, c, ref nextNodeNo, nodes, source, edgeMid);
                int mca = GetOrCreateMidpoint(c, a, ref nextNodeNo, nodes, source, edgeMid);

                // 4 sub-triangles (same winding as parent)
                elems.Add(new TriangleElement(nextElemNo++, a,   mab, mca));
                elems.Add(new TriangleElement(nextElemNo++, mab, b,   mbc));
                elems.Add(new TriangleElement(nextElemNo++, mca, mbc, c  ));
                elems.Add(new TriangleElement(nextElemNo++, mab, mbc, mca));
            }

            var result = new Model
            {
                NodesNo        = nodes.Count,
                ElementsNo     = elems.Count,
                LoadsNo        = source.LoadsNo,
                SupportsNo     = source.SupportsNo,
                YoungModulus   = source.YoungModulus,
                PoissonRatio   = source.PoissonRatio,
                Thickness      = source.Thickness,
                BcMethod       = source.BcMethod,
                Nodes          = nodes.ToArray(),
                Elements       = elems.ToArray(),
                PointLoads     = source.PointLoads,   // loads stay on original nodes
                Supports       = source.Supports       // supports stay on original nodes
            };

            return result;
        }

        private static int GetOrCreateMidpoint(
            int a, int b,
            ref int nextNodeNo,
            List<Node> nodes,
            Model source,
            Dictionary<long, int> edgeMid)
        {
            long key = EdgeKey(a, b);
            if (edgeMid.TryGetValue(key, out int mid))
                return mid;

            // Create midpoint node
            double x = (source.Nodes[a - 1].X + source.Nodes[b - 1].X) * 0.5;
            double y = (source.Nodes[a - 1].Y + source.Nodes[b - 1].Y) * 0.5;
            nodes.Add(new Node(nextNodeNo, x, y));
            edgeMid[key] = nextNodeNo;
            return nextNodeNo++;
        }

        // Symmetric edge key: same value for (a,b) and (b,a)
        private static long EdgeKey(int a, int b) =>
            a < b ? ((long)a << 32) | (uint)b
                  : ((long)b << 32) | (uint)a;
    }
}

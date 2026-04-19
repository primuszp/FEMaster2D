using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FEMaster.Core.Extensions;

namespace FEMaster.Core
{
    public class Model
    {
        #region Properties

        public int NodesNo { get; internal set; }

        public int LoadsNo { get; internal set; }

        public int ElementsNo { get; internal set; }

        public int SupportsNo { get; internal set; }

        public double Thickness { get; set; }

        public double YoungModulus { get; set; }

        public double PoissonRatio { get; set; }

        public Node[] Nodes { get; set; }

        public TriangleElement[] Elements { get; set; }

        public PointLoad[] PointLoads { get; set; }

        public Support[] Supports { get; set; }

        #endregion

        public Range SigmaXRange { get; private set; }
        public Range SigmaYRange { get; private set; }
        public Range EpsilonXRange { get; private set; }
        public Range EpsilonYRange { get; private set; }
        public Range TauXYRange { get; private set; }
        public Range GammaXYRange { get; private set; }

        public double[] Deformations;

        public bool HasResults => Deformations != null && Deformations.Length > 0;

        public BoundaryConditionMethod BcMethod { get; set; } = BoundaryConditionMethod.DirectCondensation;

        public double GetDeformationX(int nodeIndex) => HasResults ? Deformations[nodeIndex * 2] : 0;
        public double GetDeformationY(int nodeIndex) => HasResults ? Deformations[nodeIndex * 2 + 1] : 0;

        public void Analyse()
        {
            int nDof = NodesNo * 2;

            double[] r = new double[nDof];
            for (int i = 0; i < LoadsNo; i++)
            {
                r[getDOFx(PointLoads[i].NodeNo - 1)] += PointLoads[i].Fx;
                r[getDOFy(PointLoads[i].NodeNo - 1)] += PointLoads[i].Fy;
            }

            // Direct condensation is the primary and numerically preferred path.
            // The penalty path is retained only as a legacy fallback hook.
            Deformations = SolveDirectCondensation(r);

            // Now that we have deformations, lets calculate stresses and strains..
            SigmaXRange = new Range(); SigmaYRange = new Range(); TauXYRange = new Range();
            EpsilonXRange = new Range(); EpsilonYRange = new Range(); GammaXYRange = new Range();

            // first is to calculate strains
            // strain = [B] * d.
            // since the element is a constant strain triangle, the strain value will be 
            // constant for each element

            for (int i = 0; i < ElementsNo; i++)
            {
                var cst = new CsTriangle(
                    Nodes[Elements[i].Node1 - 1].X, Nodes[Elements[i].Node1 - 1].Y,
                    Nodes[Elements[i].Node2 - 1].X, Nodes[Elements[i].Node2 - 1].Y,
                    Nodes[Elements[i].Node3 - 1].X, Nodes[Elements[i].Node3 - 1].Y,
                    Thickness, YoungModulus, PoissonRatio);

                int[] dofs = GetElementDofs(i);
                double[] disp = new double[6];
                for (int j = 0; j < 6; j++)
                    disp[j] = Deformations[dofs[j]];

                double[,] B = cst.ComputeBMatrix();
                Elements[i].Strains = MultiplyMatrixWithVector(ref B, ref disp);

                double[,] D = cst.ComputeDMatrix();
                Elements[i].Stresses = MultiplyMatrixWithVector(ref D, ref Elements[i].Strains);
                SigmaXRange.AddValue(Elements[i].Stresses[0]);
                SigmaYRange.AddValue(Elements[i].Stresses[1]);
                TauXYRange.AddValue(Elements[i].Stresses[2]);

                EpsilonXRange.AddValue(Elements[i].Strains[0]);
                EpsilonYRange.AddValue(Elements[i].Strains[1]);
                GammaXYRange.AddValue(Elements[i].Strains[2]);
            }
        }

        private double[,] BuildElementStiffness(int i)
        {
            return new CsTriangle(
                Nodes[Elements[i].Node1 - 1].X, Nodes[Elements[i].Node1 - 1].Y,
                Nodes[Elements[i].Node2 - 1].X, Nodes[Elements[i].Node2 - 1].Y,
                Nodes[Elements[i].Node3 - 1].X, Nodes[Elements[i].Node3 - 1].Y,
                Thickness, YoungModulus, PoissonRatio).ComputeStiffnessMatrix();
        }

        private int[] GetElementDofs(int elemIdx)
        {
            return new[]
            {
                getDOFx(Elements[elemIdx].Node1 - 1), getDOFy(Elements[elemIdx].Node1 - 1),
                getDOFx(Elements[elemIdx].Node2 - 1), getDOFy(Elements[elemIdx].Node2 - 1),
                getDOFx(Elements[elemIdx].Node3 - 1), getDOFy(Elements[elemIdx].Node3 - 1)
            };
        }

        // Penalty method: multiplies constrained diagonal entries by a large factor.
        // Simple but increases condition number by ~10^10.
        private double[] SolvePenalty(double[,] Kg, double[] r)
        {
            double pow   = MaxKgiiPower(ref Kg);
            double large = Math.Pow(10, Math.Ceiling(pow) + 10);
            if (double.IsNaN(large) || double.IsInfinity(large))
                large = 1e20;

            for (int i = 0; i < SupportsNo; i++)
            {
                if (Supports[i].RestraintX == 1)
                    Kg[getDOFx(Supports[i].NodeNo - 1), 0] *= large;
                if (Supports[i].RestraintY == 1)
                    Kg[getDOFy(Supports[i].NodeNo - 1), 0] *= large;
            }

            var gauss = new Gauss(Kg);
            return gauss.Solve(r);
        }

        // Direct condensation: removes constrained DOFs, assembles sparse K_ff, solves with PCG.
        // Memory is O(nnz) ≈ O(6·nFree) for triangular meshes — safe after many refinements.
        private double[] SolveDirectCondensation(double[] r)
        {
            int nDof = NodesNo * 2;

            bool[] constrained = new bool[nDof];
            for (int i = 0; i < SupportsNo; i++)
            {
                if (Supports[i].RestraintX == 1) constrained[getDOFx(Supports[i].NodeNo - 1)] = true;
                if (Supports[i].RestraintY == 1) constrained[getDOFy(Supports[i].NodeNo - 1)] = true;
            }

            int nFree = 0;
            int[] map = new int[nDof];
            for (int i = 0; i < nDof; i++)
                map[i] = constrained[i] ? -1 : nFree++;

            var builder = new SparseMatrix.Builder(nFree);
            double[] rf = new double[nFree];

            for (int e = 0; e < ElementsNo; e++)
            {
                double[,] ke   = BuildElementStiffness(e);
                int[]     dofs = GetElementDofs(e);

                for (int a = 0; a < 6; a++)
                {
                    int ra = map[dofs[a]];
                    if (ra < 0) continue;
                    for (int b = 0; b < 6; b++)
                    {
                        int rb = map[dofs[b]];
                        if (rb < 0) continue;
                        // Feed only upper triangle; Build() mirrors each off-diagonal entry.
                        // Adding both (ra,rb) and (rb,ra) would double-count off-diagonals.
                        if (ra <= rb)
                            builder.Add(ra, rb, ke[a, b]);
                    }
                }
            }

            for (int i = 0; i < nDof; i++)
                if (map[i] >= 0) rf[map[i]] = r[i];

            double[] uf = PCG.Solve(builder.Build(), rf);

            double[] u = new double[nDof];
            for (int i = 0; i < nDof; i++)
                if (map[i] >= 0) u[i] = uf[map[i]];

            return u;
        }

        private double MaxKgiiPower(ref double[,] Kg)
        {
            double max = 0;

            for (int i = 0; i <= Kg.GetLength(0) - 1; i++)
            {
                var value = Math.Abs(Kg[i, 0]);
                if (value > max)
                    max = value;
            }
            if (max <= double.Epsilon)
                return 0;
            double p = Math.Log10(max);
            return p;
        }

        private bool TestKe(ref double[,] Ke)
        {
            for (var i = 0; i <= 5; i++)
            {
                if (Ke[i, i] < 0.0000001)
                    return false;
            }

            for (var i = 0; i <= 5; i++)
            {
                for (var j = 0; j <= 5; j++)
                {
                    if (Math.Abs(Ke[i, j] - Ke[j, i]) > 0.00001)
                        return false;
                }
            }
            return true;
        }

        private void AssembleKg(ref double[,] Ke, ref double[,] Kg, int ElementNo)
        {
            int[] dofs =
            {
                getDOFx(Elements[ElementNo].Node1 - 1),
                getDOFy(Elements[ElementNo].Node1 - 1),
                getDOFx(Elements[ElementNo].Node2 - 1),
                getDOFy(Elements[ElementNo].Node2 - 1),
                getDOFx(Elements[ElementNo].Node3 - 1),
                getDOFy(Elements[ElementNo].Node3 - 1)
            };

            // Place the upper triangle of the elemental stiffness matrix in the global matrix in proper position

            for (int i = 0; i <= 5; i++) // each dof of the ke
            {
                var dofi = dofs[i];

                for (int j = 0; j <= 5; j++)
                {
                    var dofj = dofs[j] - dofi;

                    if (dofj >= 0)
                        Kg[dofi, dofj] = Kg[dofi, dofj] + Ke[i, j];

                    //var dofj = dofs[j];
                    //Kg[dofi, dofj] = Kg[dofi, dofj] + Ke[i, j];
                }
            }
        }

        private int getDOFx(int NodeNo)
        {
            int nDofsPerNode = 2;
            return (NodeNo) * nDofsPerNode;
        }

        private int getDOFy(int NodeNo)
        {
            int nDofsPerNode = 2;
            return NodeNo * nDofsPerNode + 1;
        }

        private int getHalfBandWidth()
        {
            int maxDiff = 0;
            int[] n = new int[3];

            for (int i = 0; i < ElementsNo; i++)
            {
                n[0] = Elements[i].Node1;
                n[1] = Elements[i].Node2;
                n[2] = Elements[i].Node3;

                var diff = n.Max() - n.Min();

                if (maxDiff < diff)
                    maxDiff = diff;
            }

            // now we have maxdiff
            // half band width is maxdiff * 2. 2 because there are 2 dofs per node

            int hbw = (maxDiff + 1) * 2;

            if (hbw > NodesNo * 2)
                hbw = NodesNo * 2;

            return hbw;
        }

        /// <summary>
        /// Multiplies matrix a by vector b and returns the product
        ///</summary>
        ///<param name="a"></param>
        ///<param name="b"></param>
        ///<returns></returns>
        private double[] MultiplyMatrixWithVector(ref double[,] a, ref double[] b)
        {
            int aRows = a.GetLength(0);
            int aCols = a.GetLength(1);
            double[] ab = new double[aRows]; // output will be a vector
            for (int i = 0; i <= aRows - 1; i++)
            {
                ab[i] = 0.0;
                for (int j = 0; j <= aCols - 1; j++)
                    ab[i] += a[i, j] * b[j];
            }

            return ab;
        }

        #region Model Report

        public string Report()
        {
            string s;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Model Statistics:");
            sb.AppendLine("Number of Nodes: " + NodesNo);
            sb.AppendLine("Number of Elements: " + ElementsNo);
            sb.AppendLine("Number of Variables: " + NodesNo * 2);
            sb.AppendLine("");

            // Deformations

            sb.AppendLine("Deformations:");
            sb.AppendLine("Node" + "\t" + "\t" + "Ux" + "\t" + "\t" + "Uy");

            for (var i = 0; i < NodesNo; i++)
            {
                s = i + 1 + "\t" + "\t";
                var dof = getDOFx(i);
                s = s + Deformations[dof] + "\t" + "\t";
                s = s + Deformations[dof + 1];

                sb.AppendLine(s);
            }

            // Element stresses and strains

            sb.AppendLine("Stresses and Strains:");
            sb.AppendLine("Element" + "\t" + "\t" + "sx" + "\t" + "\t" + "sy" + "\t" + "\t" + "txy" + "\t" + "\t" + "ex" + "\t" + "\t" + "ey" + "\t" + "\t" + "gamma_xy");

            for (var i = 0; i < ElementsNo; i++)
            {
                s = i + 1 + "\t" + "\t";

                s = s + Elements[i].Stresses[0] + "\t" + "\t";
                s = s + Elements[i].Stresses[1] + "\t" + "\t";
                s = s + Elements[i].Stresses[2] + "\t" + "\t";

                s = s + Elements[i].Strains[0] + "\t" + "\t";
                s = s + Elements[i].Strains[1] + "\t" + "\t";
                s = s + Elements[i].Strains[2];

                sb.AppendLine(s);
            }

            // Write maximum displacements

            var uxMax = double.MinValue;
            var uyMax = double.MinValue;
            var uxMin = double.MaxValue;
            var uyMin = double.MaxValue;

            for (var i = 0; i < Deformations.Length; i += 2)
            {
                if (uxMax < Deformations[i])
                    uxMax = Deformations[i];

                if (uxMin > Deformations[i])
                    uxMin = Deformations[i];

                if (uyMax < Deformations[i + 1])
                    uyMax = Deformations[i + 1];

                if (uyMin > Deformations[i + 1])
                    uyMin = Deformations[i + 1];
            }

            sb.AppendLine("");
            sb.AppendLine("Maximum Displacement in X direction = " + uxMax.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Minimum Displacement in X direction = " + uxMin.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Maximum Displacement in Y direction = " + uyMax.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Minimum Displacement in Y direction = " + uyMin.ToString(CultureInfo.InvariantCulture));

            sb.AppendLine("");
            sb.AppendLine("Output generated by btFEM at " + DateTime.Now.ToLongDateString());

            return sb.ToString();
        }

        #endregion

        #region Static Methods

        public static Model Load(Stream stream, Action<Exception> error = null)
        {
            var model = new Model();

            using (StreamReader reader = new StreamReader(stream))
            {
                try
                {
                    ReadModelHeader(reader, model, error);
                    ReadModelNodeItems(reader, model, error);
                    ReadModelElementItems(reader, model, error);
                    ReadModelLoadItems(reader, model, error);
                    ReadModelSupportItems(reader, model, error);

                    if (!IsValid(model))
                    {
                        error?.Invoke(new InvalidDataException("Model file is incomplete or contains invalid counts."));
                        return null;
                    }
                }
                catch (Exception exception)
                {
                    error?.Invoke(exception);
                    return null;
                }
            }

            return model;
        }

        private static bool IsValid(Model model)
        {
            return model != null
                && model.NodesNo > 0
                && model.ElementsNo > 0
                && model.LoadsNo >= 0
                && model.SupportsNo >= 0
                && model.Nodes != null && model.Nodes.Length == model.NodesNo
                && model.Elements != null && model.Elements.Length == model.ElementsNo
                && model.PointLoads != null && model.PointLoads.Length == model.LoadsNo
                && model.Supports != null && model.Supports.Length == model.SupportsNo;
        }

        private static void ReadModelSupportItems(StreamReader reader, Model model, Action<Exception> error = null)
        {
            try
            {
                model.Supports = new Support[model.SupportsNo];

                for (int i = 0; i < model.SupportsNo; i++)
                {
                    var line = ReadLine(reader).Replace("\t", string.Empty);

                    try
                    {
                        var buffer = line.Split(',');

                        var sn = buffer[0].ToInt();
                        var nn = buffer[1].ToInt();
                        var rx = buffer[2].ToInt();
                        var ry = buffer[3].ToInt();

                        model.Supports[i] = new Support(sn, nn, rx, ry);
                    }
                    catch (Exception exception)
                    {
                        error?.Invoke(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                error?.Invoke(exception);
            }
        }

        private static void ReadModelLoadItems(StreamReader reader, Model model, Action<Exception> error = null)
        {
            try
            {
                model.PointLoads = new PointLoad[model.LoadsNo];

                for (int i = 0; i < model.LoadsNo; i++)
                {
                    var line = ReadLine(reader).Replace("\t", string.Empty);

                    try
                    {
                        var buffer = line.Split(',');

                        var ln = buffer[0].ToInt();
                        var nn = buffer[1].ToInt();
                        var fx = buffer[2].ToDouble();
                        var fy = buffer[3].ToDouble();

                        model.PointLoads[i] = new PointLoad(ln, nn, fx, fy);
                    }
                    catch (Exception exception)
                    {
                        error?.Invoke(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                error?.Invoke(exception);
            }
        }

        private static void ReadModelElementItems(StreamReader reader, Model model, Action<Exception> error = null)
        {
            try
            {
                model.Elements = new TriangleElement[model.ElementsNo];

                for (int i = 0; i < model.ElementsNo; i++)
                {
                    var line = ReadLine(reader).Replace("\t", string.Empty);

                    try
                    {
                        var buffer = line.Split(',');

                        var nn = buffer[0].ToInt();
                        var n1 = buffer[1].ToInt();
                        var n2 = buffer[2].ToInt();
                        var n3 = buffer[3].ToInt();

                        model.Elements[i] = new TriangleElement(nn, n1, n2, n3);
                    }
                    catch (Exception exception)
                    {
                        error?.Invoke(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                error?.Invoke(exception);
            }
        }

        private static void ReadModelNodeItems(StreamReader reader, Model model, Action<Exception> error = null)
        {
            try
            {
                model.Nodes = new Node[model.NodesNo];

                for (int i = 0; i < model.NodesNo; i++)
                {
                    var line = ReadLine(reader).Replace("\t", string.Empty);

                    try
                    {
                        var buffer = line.Split(',');

                        var nn = buffer[0].ToInt();
                        var nx = buffer[1].ToDouble();
                        var ny = buffer[2].ToDouble();

                        model.Nodes[i] = new Node(nn, nx, ny);
                    }
                    catch (Exception exception)
                    {
                        error?.Invoke(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                error?.Invoke(exception);
            }
        }

        private static void ReadModelHeader(StreamReader reader, Model model, Action<Exception> error = null)
        {
            try
            {
                string line = ReadLine(reader);
                string[] buffer = line.Split(',');

                model.NodesNo = buffer[0].ToInt(); model.ElementsNo = buffer[1].ToInt();
                model.LoadsNo = buffer[2].ToInt(); model.SupportsNo = buffer[3].ToInt();

                model.YoungModulus = buffer[4].ToDouble();
                model.PoissonRatio = buffer[5].ToDouble();
                model.Thickness = buffer[6].ToDouble();
            }
            catch (Exception exception)
            {
                error?.Invoke(exception);
            }
        }

        private static string ReadLine(StreamReader reader)
        {
            var line = string.Empty;

            if (reader.EndOfStream)
            {
                return line;
            }

            while (reader.EndOfStream == false)
            {
                line = reader.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(line)) continue;

                if (line.Substring(0, 1) != "%")
                {
                    return line;
                }
            }

            return line;
        }

        #endregion
    }
}

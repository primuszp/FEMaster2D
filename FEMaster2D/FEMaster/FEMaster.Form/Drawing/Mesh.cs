using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FEMaster.Core;
using FEMaster.Form.Contracts;

namespace FEMaster.Form.Drawing
{
    public enum ResultType
    {
        None,
        SigmaX,
        SigmaY,
        TauXY,
        EpsilonX,
        EpsilonY,
        GammaXY
    }

    public class ElementSelectedEventArgs : EventArgs
    {
        public int ElementIndex { get; }
        public ElementSelectedEventArgs(int index) { ElementIndex = index; }
    }

    public class MeshEntity : Drawable, IMouseListener
    {
        private Model model;
        private Func<Point2D, PointF>  _worldToScreen;
        private Func<PointF, Point2D>  _screenToWorld;
        private int _selectedElement = -1;

        public event EventHandler<ElementSelectedEventArgs> ElementSelected;

        public ResultType ShowResult    { get; set; } = ResultType.None;
        public double DeformationZoom   { get; set; } = 1.0;
        public bool ShowUndeformed      { get; set; } = true;
        public bool ShowDeformed        { get; set; } = true;
        public bool ShowNodeNumbers     { get; set; } = false;
        public bool ShowElementEdges    { get; set; } = true;

        public MeshEntity(Model model) { this.model = model; }

        public void UpdateModel(Model m) { model = m; _selectedElement = -1; Invalidate(); }

        public void AutoScaleDeformationZoom()
        {
            if (model == null || !model.HasResults) return;
            double maxDisp = 0;
            for (int i = 0; i < model.NodesNo; i++)
            {
                double dx = Math.Abs(model.GetDeformationX(i));
                double dy = Math.Abs(model.GetDeformationY(i));
                if (dx > maxDisp) maxDisp = dx;
                if (dy > maxDisp) maxDisp = dy;
            }
            if (maxDisp < double.Epsilon) { DeformationZoom = 1; return; }
            double xMin = double.MaxValue, xMax = double.MinValue;
            double yMin = double.MaxValue, yMax = double.MinValue;
            for (int i = 0; i < model.NodesNo; i++)
            {
                if (model.Nodes[i].X < xMin) xMin = model.Nodes[i].X;
                if (model.Nodes[i].X > xMax) xMax = model.Nodes[i].X;
                if (model.Nodes[i].Y < yMin) yMin = model.Nodes[i].Y;
                if (model.Nodes[i].Y > yMax) yMax = model.Nodes[i].Y;
            }
            DeformationZoom = Math.Max(xMax - xMin, yMax - yMin) * 0.05 / maxDisp;
        }

        public override void Draw(Canvas canvas)
        {
            if (model == null) return;

            _worldToScreen = canvas.WorldToScreen;
            _screenToWorld = canvas.ScreenToWorld;

            canvas.GraphicsPipeline(TargetSpace.WorldSpace, 0, g =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (ShowUndeformed)      DrawUndeformedMesh(g, canvas);
                if (model.HasResults && ShowDeformed) DrawDeformedMesh(g, canvas);
                if (_selectedElement >= 0) DrawSelectedElement(g, canvas);
                DrawSupports(g, canvas);
                DrawLoads(g, canvas);
                if (ShowNodeNumbers)     DrawNodeNumbers(g, canvas);
            });

            if (model.HasResults && ShowResult != ResultType.None)
                canvas.GraphicsPipeline(TargetSpace.ScreenSpaceOverlay, 0, g =>
                    DrawColorBar(g, GetActiveRange()));
        }

        // ── IMouseListener ───────────────────────────────────────────────────

        public void OnMouseButtonDown(MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left || model == null || _screenToWorld == null) return;

            var world = _screenToWorld(args.Location);
            int hit = FindElementAt(world.X, world.Y);

            if (hit == _selectedElement) return;
            _selectedElement = hit;
            ElementSelected?.Invoke(this, new ElementSelectedEventArgs(hit));
            Invalidate();
        }

        public void OnMouseButtonUp(MouseEventArgs args) { }
        public void OnMouseMove(MouseEventArgs args) { }
        public void OnMouseWheel(MouseEventArgs args) { }

        // ── Hit testing ──────────────────────────────────────────────────────

        private int FindElementAt(double wx, double wy)
        {
            for (int i = 0; i < model.ElementsNo; i++)
            {
                int n1 = model.Elements[i].Node1 - 1;
                int n2 = model.Elements[i].Node2 - 1;
                int n3 = model.Elements[i].Node3 - 1;
                double ax = model.Nodes[n1].X, ay = model.Nodes[n1].Y;
                double bx = model.Nodes[n2].X, by = model.Nodes[n2].Y;
                double cx = model.Nodes[n3].X, cy = model.Nodes[n3].Y;
                if (PointInTriangle(wx, wy, ax, ay, bx, by, cx, cy))
                    return i;
            }
            return -1;
        }

        private static bool PointInTriangle(double px, double py,
            double ax, double ay, double bx, double by, double cx, double cy)
        {
            double d1 = Cross(px, py, ax, ay, bx, by);
            double d2 = Cross(px, py, bx, by, cx, cy);
            double d3 = Cross(px, py, cx, cy, ax, ay);
            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        private static double Cross(double px, double py,
            double ax, double ay, double bx, double by)
            => (px - bx) * (ay - by) - (ax - bx) * (py - by);

        // ── Mesh ─────────────────────────────────────────────────────────────

        private void DrawUndeformedMesh(Graphics g, Canvas canvas)
        {
            using (var fill = new SolidBrush(Color.FromArgb(230, 230, 250)))
            using (var pen  = new Pen(Color.FromArgb(160, 160, 200), 0.8f))
            {
                for (int i = 0; i < model.ElementsNo; i++)
                {
                    var pts = ScreenPts(i, canvas, 0);
                    g.FillPolygon(fill, pts);
                    g.DrawPolygon(pen, pts);
                }
            }
        }

        private void DrawDeformedMesh(Graphics g, Canvas canvas)
        {
            var range    = GetActiveRange();
            var colorMap = range != null ? new ColorMap(range.Max, range.Min) : new ColorMap(1, 0);

            using (var edgePen = new Pen(Color.FromArgb(80, 80, 80), 0.5f))
            {
                for (int i = 0; i < model.ElementsNo; i++)
                {
                    Color fill = ShowResult == ResultType.None ? Color.Turquoise : GetElementColor(colorMap, i);
                    var pts = ScreenPts(i, canvas, DeformationZoom);
                    using (var brush = new SolidBrush(fill))
                        g.FillPolygon(brush, pts);
                    if (ShowElementEdges)
                        g.DrawPolygon(edgePen, pts);
                }
            }
        }

        private void DrawSelectedElement(Graphics g, Canvas canvas)
        {
            var pts = ScreenPts(_selectedElement, canvas, 0);
            using (var fill = new SolidBrush(Color.FromArgb(80, 255, 220, 0)))
            using (var pen  = new Pen(Color.Gold, 2f))
            {
                g.FillPolygon(fill, pts);
                g.DrawPolygon(pen, pts);
            }
        }

        private PointF[] ScreenPts(int i, Canvas canvas, double dz)
        {
            int n1 = model.Elements[i].Node1 - 1;
            int n2 = model.Elements[i].Node2 - 1;
            int n3 = model.Elements[i].Node3 - 1;
            return new[]
            {
                canvas.WorldToScreen(new Point2D(model.Nodes[n1].X + model.GetDeformationX(n1)*dz,
                                                 model.Nodes[n1].Y + model.GetDeformationY(n1)*dz)),
                canvas.WorldToScreen(new Point2D(model.Nodes[n2].X + model.GetDeformationX(n2)*dz,
                                                 model.Nodes[n2].Y + model.GetDeformationY(n2)*dz)),
                canvas.WorldToScreen(new Point2D(model.Nodes[n3].X + model.GetDeformationX(n3)*dz,
                                                 model.Nodes[n3].Y + model.GetDeformationY(n3)*dz))
            };
        }

        // ── Supports ─────────────────────────────────────────────────────────
        //
        //  Pin  (rx && ry): triangle + circle + hatching both directions
        //  Roller-Y (ry only, free in X): triangle + roller circles + horizontal ground
        //  Roller-X (rx only, free in Y): rotated triangle + roller circles + vertical ground

        private void DrawSupports(Graphics g, Canvas canvas)
        {
            using (var outline = new Pen(Color.White, 3.6f))
            using (var pen     = new Pen(Color.FromArgb(60, 120, 220), 1.8f))
            using (var fill    = new SolidBrush(Color.FromArgb(80, 60, 120, 220)))
            {
                for (int i = 0; i < model.SupportsNo; i++)
                {
                    int n  = model.Supports[i].NodeNo - 1;
                    var pt = canvas.WorldToScreen(new Point2D(model.Nodes[n].X, model.Nodes[n].Y));
                    bool rx = model.Supports[i].RestraintX == 1;
                    bool ry = model.Supports[i].RestraintY == 1;

                    if (rx && ry)
                    {
                        DrawSupportPin(g, outline, null, pt);
                        DrawSupportPin(g, pen, fill, pt);
                    }
                    else if (ry)
                    {
                        DrawSupportRollerY(g, outline, null, pt);
                        DrawSupportRollerY(g, pen, fill, pt);
                    }
                    else if (rx)
                    {
                        DrawSupportRollerX(g, outline, null, pt);
                        DrawSupportRollerX(g, pen, fill, pt);
                    }
                }
            }
        }

        // Pin: triangle (apex=node) + circle at node + hatching both horizontal and vertical
        private static void DrawSupportPin(Graphics g, Pen pen, SolidBrush fill, PointF pt)
        {
            const int r  = 4;   // node circle radius
            const int s  = 10;  // triangle half-base
            const int h  = 10;  // triangle height
            const int gl = 14;  // ground-line half-length
            const int gs = 5;   // hatch spacing

            var tri = new[] { pt, new PointF(pt.X - s, pt.Y + h), new PointF(pt.X + s, pt.Y + h) };
            if (fill != null) g.FillPolygon(fill, tri);
            g.DrawPolygon(pen, tri);
            g.DrawEllipse(pen, pt.X - r, pt.Y - r, r * 2, r * 2);

            // horizontal ground line + hatching
            float gy = pt.Y + h;
            g.DrawLine(pen, pt.X - gl, gy, pt.X + gl, gy);
            for (int x = -gl; x <= gl - gs; x += gs)
                g.DrawLine(pen, pt.X + x, gy, pt.X + x - 4, gy + 5);

            // vertical ground line + hatching (left side)
            float gx = pt.X - s - 2;
            g.DrawLine(pen, gx, pt.Y - gl + h, gx, pt.Y + gl + h);
            for (int y = -gl + h; y <= gl + h - gs; y += gs)
                g.DrawLine(pen, gx, pt.Y + y, gx - 5, pt.Y + y + 4);
        }

        // Roller-Y: free in X, fixed in Y — triangle + 3 roller circles + horizontal ground
        private static void DrawSupportRollerY(Graphics g, Pen pen, SolidBrush fill, PointF pt)
        {
            const int r  = 4;
            const int s  = 10;
            const int h  = 10;
            const int gl = 14;
            const int cr = 3;   // roller circle radius
            const int rg = 3;   // gap between triangle base and rollers

            var tri = new[] { pt, new PointF(pt.X - s, pt.Y + h), new PointF(pt.X + s, pt.Y + h) };
            if (fill != null) g.FillPolygon(fill, tri);
            g.DrawPolygon(pen, tri);
            g.DrawEllipse(pen, pt.X - r, pt.Y - r, r * 2, r * 2);

            // roller circles at base
            float ry2 = pt.Y + h + rg;
            for (int k = -1; k <= 1; k++)
                g.DrawEllipse(pen, pt.X + k * (cr * 2 + 1) - cr, ry2, cr * 2, cr * 2);

            // horizontal ground line below rollers
            float gy = ry2 + cr * 2;
            g.DrawLine(pen, pt.X - gl, gy, pt.X + gl, gy);
        }

        // Roller-X: free in Y, fixed in X — rotated triangle (pointing left) + roller circles + vertical ground
        private static void DrawSupportRollerX(Graphics g, Pen pen, SolidBrush fill, PointF pt)
        {
            const int r  = 4;
            const int s  = 10;
            const int h  = 10;
            const int gl = 14;
            const int cr = 3;
            const int rg = 3;

            // triangle pointing left: apex = node, base to the right
            var tri = new[] { pt, new PointF(pt.X + h, pt.Y - s), new PointF(pt.X + h, pt.Y + s) };
            if (fill != null) g.FillPolygon(fill, tri);
            g.DrawPolygon(pen, tri);
            g.DrawEllipse(pen, pt.X - r, pt.Y - r, r * 2, r * 2);

            // roller circles to the right of base
            float rx2 = pt.X + h + rg;
            for (int k = -1; k <= 1; k++)
                g.DrawEllipse(pen, rx2, pt.Y + k * (cr * 2 + 1) - cr, cr * 2, cr * 2);

            // vertical ground line to the right of rollers
            float gx = rx2 + cr * 2;
            g.DrawLine(pen, gx, pt.Y - gl, gx, pt.Y + gl);
        }

        // ── Loads ─────────────────────────────────────────────────────────────

        private void DrawLoads(Graphics g, Canvas canvas)
        {
            using (var outline = new Pen(Color.White, 4f))
            using (var pen     = new Pen(Color.Red, 2f))
            using (var fill    = new SolidBrush(Color.FromArgb(220, Color.Red)))
            {
                for (int i = 0; i < model.LoadsNo; i++)
                {
                    int n  = model.PointLoads[i].NodeNo - 1;
                    var pt = canvas.WorldToScreen(new Point2D(model.Nodes[n].X, model.Nodes[n].Y));

                    if (Math.Abs(model.PointLoads[i].Fx) > 0.0001)
                    {
                        float ang = model.PointLoads[i].Fx > 0 ? 0 : 180;
                        DrawArrow(g, outline, null, pt, ang);
                        DrawArrow(g, pen, fill, pt, ang);
                    }

                    if (Math.Abs(model.PointLoads[i].Fy) > 0.0001)
                    {
                        float ang = model.PointLoads[i].Fy > 0 ? 90 : 270;
                        DrawArrow(g, outline, null, pt, ang);
                        DrawArrow(g, pen, fill, pt, ang);
                    }
                }
            }
        }

        private static void DrawArrow(Graphics g, Pen pen, SolidBrush fill, PointF pt, float angleDeg)
        {
            const int len   = 22;
            const int head  = 7;
            const int hWide = 5;

            double rad = angleDeg * Math.PI / 180.0;
            float  dx  = (float)Math.Cos(rad);
            float  dy  = -(float)Math.Sin(rad);

            var tail = new PointF(pt.X - dx * len, pt.Y - dy * len);
            g.DrawLine(pen, tail, new PointF(pt.X - dx * head, pt.Y - dy * head));

            float nx = -dy, ny = dx;
            var arrowHead = new[]
            {
                pt,
                new PointF(pt.X - dx * head - nx * hWide, pt.Y - dy * head - ny * hWide),
                new PointF(pt.X - dx * head + nx * hWide, pt.Y - dy * head + ny * hWide)
            };
            if (fill != null) g.FillPolygon(fill, arrowHead);
            g.DrawPolygon(pen, arrowHead);
        }

        // ── Node numbers ──────────────────────────────────────────────────────

        private void DrawNodeNumbers(Graphics g, Canvas canvas)
        {
            using (var font  = new Font("Arial", 7, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.OrangeRed))
            {
                for (int i = 0; i < model.NodesNo; i++)
                {
                    var pt = canvas.WorldToScreen(new Point2D(model.Nodes[i].X, model.Nodes[i].Y));
                    g.DrawString(model.Nodes[i].NodeNo.ToString(), font, brush,
                        new PointF(pt.X + 3, pt.Y + 2));
                }
            }
        }

        // ── Colorbar ──────────────────────────────────────────────────────────

        private Color GetElementColor(ColorMap map, int i)
        {
            switch (ShowResult)
            {
                case ResultType.SigmaX:   return map.GetColor(model.Elements[i].Stresses[0]);
                case ResultType.SigmaY:   return map.GetColor(model.Elements[i].Stresses[1]);
                case ResultType.TauXY:    return map.GetColor(model.Elements[i].Stresses[2]);
                case ResultType.EpsilonX: return map.GetColor(model.Elements[i].Strains[0]);
                case ResultType.EpsilonY: return map.GetColor(model.Elements[i].Strains[1]);
                case ResultType.GammaXY:  return map.GetColor(model.Elements[i].Strains[2]);
                default: return Color.Turquoise;
            }
        }

        private FEMaster.Core.Range GetActiveRange()
        {
            switch (ShowResult)
            {
                case ResultType.SigmaX:   return model.SigmaXRange;
                case ResultType.SigmaY:   return model.SigmaYRange;
                case ResultType.TauXY:    return model.TauXYRange;
                case ResultType.EpsilonX: return model.EpsilonXRange;
                case ResultType.EpsilonY: return model.EpsilonYRange;
                case ResultType.GammaXY:  return model.GammaXYRange;
                default: return null;
            }
        }

        private static void DrawColorBar(Graphics g, FEMaster.Core.Range range)
        {
            if (range == null) return;
            var cmap = new ColorMap(250, 0);
            const int x1 = 20, x2 = 40, yTop = 50, yBottom = 300;
            int barH = yBottom - yTop;

            for (int i = 0; i <= 250; i++)
            {
                int y = yTop + (int)((1.0 - i / 250.0) * barH);
                using (var pen = new Pen(cmap.GetColor(i)))
                    g.DrawLine(pen, x1, y, x2, y);
            }

            using (var font    = new Font("Arial", 8))
            using (var brush   = new SolidBrush(Color.White))
            using (var linePen = new Pen(Color.White))
            {
                void Tick(int y, double val)
                {
                    g.DrawLine(linePen, x2, y, x2 + 4, y);
                    g.DrawString(val.ToString("G4"), font, brush, new PointF(x2 + 6, y - 6));
                }
                Tick(yTop,            range.Max);
                Tick(yTop + barH / 2, (range.Max + range.Min) / 2);
                Tick(yBottom,         range.Min);
            }
        }
    }
}

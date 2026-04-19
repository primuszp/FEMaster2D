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
        private double[] nodalResultCache;
        private ResultType cachedResultType = ResultType.None;
        private int cachedResultEpoch = -1;
        private PointF[][] undeformedScreenCache;
        private PointF[][] deformedScreenCache;
        private PointF[][] resultScreenCache;
        private RectangleF[] undeformedBoundsCache;
        private RectangleF[] deformedBoundsCache;
        private RectangleF[] resultBoundsCache;
        private PointF[] supportScreenCache;
        private PointF[] loadScreenCache;
        private int geometryCacheKey = -1;
        private Bitmap colorBarCache;
        private ResultType colorBarCacheType = ResultType.None;
        private int colorBarCacheWidth = -1;

        public event EventHandler<ElementSelectedEventArgs> ElementSelected;

        private ResultType showResult = ResultType.None;
        public ResultType ShowResult
        {
            get => showResult;
            set
            {
                if (showResult == value) return;
                showResult = value;
                nodalResultCache = null;
                InvalidateColorBarCache();
                Invalidate();
            }
        }
        public double DeformationZoom   { get; set; } = 1.0;
        public bool ShowUndeformed      { get; set; } = true;
        public bool ShowDeformed        { get; set; } = true;
        public bool ShowNodeNumbers     { get; set; } = false;
        public bool ShowElementEdges    { get; set; } = true;

        public MeshEntity(Model model) { this.model = model; }

        public void UpdateModel(Model m)
        {
            model = m;
            _selectedElement = -1;
            geometryCacheKey = -1;
            nodalResultCache = null;
            InvalidateColorBarCache();
            Invalidate();
        }

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
            var span = Math.Max(xMax - xMin, yMax - yMin);
            if (span < double.Epsilon) span = 1;
            DeformationZoom = span * 0.05 / maxDisp;
            geometryCacheKey = -1;
        }

        public override void Draw(Canvas canvas)
        {
            if (model == null) return;

            _worldToScreen = canvas.WorldToScreen;
            _screenToWorld = canvas.ScreenToWorld;

            canvas.GraphicsPipeline(TargetSpace.WorldSpace, 0, g =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                EnsureGeometryCache(canvas);

                if (ShowUndeformed)      DrawUndeformedMesh(g);
                if (model.HasResults && ShowDeformed) DrawDeformedMesh(g);
                if (_selectedElement >= 0) DrawSelectedElement(g, canvas);
                DrawSupports(g, canvas);
                DrawLoads(g, canvas);
                if (ShowNodeNumbers)     DrawNodeNumbers(g, canvas);
            });

            if (model.HasResults && ShowResult != ResultType.None)
                canvas.GraphicsPipeline(TargetSpace.ScreenSpaceOverlay, 0, g => DrawColorBar(g));
        }

        // ── IMouseListener ───────────────────────────────────────────────────

        public void OnMouseButtonDown(MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left || model == null || _screenToWorld == null) return;

            int hit = -1;
            if (model.HasResults && ShowDeformed)
                hit = FindElementAtDeformed(args.Location);
            else if (ShowUndeformed)
                hit = FindElementAtUndeformed(args.Location);

            if (hit == _selectedElement) return;
            _selectedElement = hit;
            ElementSelected?.Invoke(this, new ElementSelectedEventArgs(hit));
            Invalidate();
        }

        public void OnMouseButtonUp(MouseEventArgs args) { }
        public void OnMouseMove(MouseEventArgs args) { }
        public void OnMouseWheel(MouseEventArgs args) { }

        // ── Hit testing ──────────────────────────────────────────────────────

        private int FindElementAtDeformed(PointF screenPoint)
        {
            return FindElementAtScreenPoint(screenPoint, true);
        }

        private int FindElementAtUndeformed(PointF screenPoint)
        {
            return FindElementAtScreenPoint(screenPoint, false);
        }

        private int FindElementAtScreenPoint(PointF screenPoint, bool deformed)
        {
            for (int i = model.ElementsNo - 1; i >= 0; i--)
            {
                var bounds = deformed ? deformedBoundsCache[i] : undeformedBoundsCache[i];
                if (!bounds.Contains(screenPoint))
                    continue;

                PointF a = deformed ? deformedScreenCache[i][0] : undeformedScreenCache[i][0];
                PointF b = deformed ? deformedScreenCache[i][1] : undeformedScreenCache[i][1];
                PointF c = deformed ? deformedScreenCache[i][2] : undeformedScreenCache[i][2];

                if (PointInTriangle(screenPoint.X, screenPoint.Y, a.X, a.Y, b.X, b.Y, c.X, c.Y))
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

        private void DrawUndeformedMesh(Graphics g)
        {
            using (var fill = new SolidBrush(Color.FromArgb(230, 230, 250)))
            using (var pen  = new Pen(Color.FromArgb(160, 160, 200), 0.8f))
            {
                for (int i = 0; i < model.ElementsNo; i++)
                {
                    var pts = undeformedScreenCache[i];
                    g.FillPolygon(fill, pts);
                    g.DrawPolygon(pen, pts);
                }
            }
        }

        private void DrawDeformedMesh(Graphics g)
        {
            var range    = GetActiveRange();
            var colorMap = range != null ? new ColorMap(range.Max, range.Min) : new ColorMap(1, 0);
            var nodalValues = GetNodalResultCache();

            using (var edgePen = new Pen(Color.FromArgb(80, 80, 80), 0.5f))
            {
                var previousSmoothing = g.SmoothingMode;
                var previousPixelOffset = g.PixelOffsetMode;

                for (int i = 0; i < model.ElementsNo; i++)
                {
                    var pts = deformedScreenCache[i];
                    if (ShowResult == ResultType.None)
                    {
                        var prevSmooth = g.SmoothingMode;
                        var prevPixel = g.PixelOffsetMode;
                        g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.None;

                        using (var brush = new SolidBrush(Color.Turquoise))
                            g.FillPolygon(brush, pts);

                        g.SmoothingMode = prevSmooth;
                        g.PixelOffsetMode = prevPixel;
                    }
                    else
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.None;

                        var p1 = resultScreenCache[i][0];
                        var p2 = resultScreenCache[i][1];
                        var p3 = resultScreenCache[i][2];
                        var n1 = model.Elements[i].Node1 - 1;
                        var n2 = model.Elements[i].Node2 - 1;
                        var n3 = model.Elements[i].Node3 - 1;
                        var c1 = colorMap.GetColor(nodalValues[n1]);
                        var c2 = colorMap.GetColor(nodalValues[n2]);
                        var c3 = colorMap.GetColor(nodalValues[n3]);
                        DrawShadedTriangle(g, p1, p2, p3, c1, c2, c3, 0);
                    }
                    if (ShowElementEdges)
                        g.DrawPolygon(edgePen, pts);
                }

                g.SmoothingMode = previousSmoothing;
                g.PixelOffsetMode = previousPixelOffset;
            }
        }

        private void DrawSelectedElement(Graphics g, Canvas canvas)
        {
            var pts = (model.HasResults && ShowDeformed)
                ? deformedScreenCache[_selectedElement]
                : undeformedScreenCache[_selectedElement];
            using (var fill = new SolidBrush(Color.FromArgb(80, 255, 220, 0)))
            using (var pen  = new Pen(Color.Gold, 2f))
            {
                g.FillPolygon(fill, pts);
                g.DrawPolygon(pen, pts);
            }
        }

        private double[] GetNodalResultCache()
        {
            if (!model.HasResults || ShowResult == ResultType.None)
                return null;

            if (nodalResultCache != null && cachedResultType == ShowResult && cachedResultEpoch == model.ElementsNo)
                return nodalResultCache;

            var sums = new double[model.NodesNo];
            var counts = new int[model.NodesNo];

            for (int i = 0; i < model.ElementsNo; i++)
            {
                double value = GetElementScalar(i);
                int n1 = model.Elements[i].Node1 - 1;
                int n2 = model.Elements[i].Node2 - 1;
                int n3 = model.Elements[i].Node3 - 1;

                sums[n1] += value; counts[n1]++;
                sums[n2] += value; counts[n2]++;
                sums[n3] += value; counts[n3]++;
            }

            nodalResultCache = new double[model.NodesNo];
            for (int i = 0; i < model.NodesNo; i++)
                nodalResultCache[i] = counts[i] > 0 ? sums[i] / counts[i] : 0.0;

            cachedResultType = ShowResult;
            cachedResultEpoch = model.ElementsNo;
            return nodalResultCache;
        }

        private double GetElementScalar(int i)
        {
            switch (ShowResult)
            {
                case ResultType.SigmaX: return model.Elements[i].Stresses[0];
                case ResultType.SigmaY: return model.Elements[i].Stresses[1];
                case ResultType.TauXY: return model.Elements[i].Stresses[2];
                case ResultType.EpsilonX: return model.Elements[i].Strains[0];
                case ResultType.EpsilonY: return model.Elements[i].Strains[1];
                case ResultType.GammaXY: return model.Elements[i].Strains[2];
                default: return 0.0;
            }
        }

        private PointF WorldPoint(int nodeIndex, double dz)
        {
            return _worldToScreen(new Point2D(
                model.Nodes[nodeIndex].X + model.GetDeformationX(nodeIndex) * dz,
                model.Nodes[nodeIndex].Y + model.GetDeformationY(nodeIndex) * dz));
        }

        private void DrawShadedTriangle(Graphics g, PointF p1, PointF p2, PointF p3, Color c1, Color c2, Color c3, int depth)
        {
            if (depth >= 2)
            {
                FillTriangle(g, p1, p2, p3, Blend(Blend(c1, c2), c3));
                return;
            }

            PointF m12 = Mid(p1, p2);
            PointF m23 = Mid(p2, p3);
            PointF m31 = Mid(p3, p1);

            Color c12 = Blend(c1, c2);
            Color c23 = Blend(c2, c3);
            Color c31 = Blend(c3, c1);
            Color center = Blend(Blend(c1, c2), c3);

            DrawShadedTriangle(g, p1, m12, m31, c1, c12, c31, depth + 1);
            DrawShadedTriangle(g, m12, p2, m23, c12, c2, c23, depth + 1);
            DrawShadedTriangle(g, m31, m23, p3, c31, c23, c3, depth + 1);
            DrawShadedTriangle(g, m12, m23, m31, c12, c23, c31, depth + 1);
        }

        private static void FillTriangle(Graphics g, PointF a, PointF b, PointF c, Color fill)
        {
            using (var brush = new SolidBrush(fill))
                g.FillPolygon(brush, new[] { a, b, c });
        }

        private static PointF Mid(PointF a, PointF b) => new PointF((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);

        private static Color Blend(Color a, Color b)
            => Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);

        // ── Supports ─────────────────────────────────────────────────────────
        //
        //  Pin  (rx && ry): triangle + circle + hatching both directions
        //  Roller-Y (ry only, free in X): triangle + roller circles + horizontal ground
        //  Roller-X (rx only, free in Y): rotated triangle + roller circles + vertical ground

        private void DrawSupports(Graphics g, Canvas canvas)
        {
            using (var outline = new Pen(Color.White, 3.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pen     = new Pen(Color.FromArgb(60, 120, 220), 1.8f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var fill    = new SolidBrush(Color.FromArgb(80, 60, 120, 220)))
            {
                for (int i = 0; i < model.SupportsNo; i++)
                {
                    var pt = supportScreenCache[i];
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
            using (var outline = new Pen(Color.White, 4f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pen     = new Pen(Color.Red, 2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var fill    = new SolidBrush(Color.FromArgb(220, Color.Red)))
            {
                for (int i = 0; i < model.LoadsNo; i++)
                {
                    var pt = loadScreenCache[i];

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

        private void DrawColorBar(Graphics g)
        {
            var range = GetActiveRange();
            if (range == null) return;

            if (colorBarCache == null || colorBarCacheType != ShowResult || colorBarCacheWidth != 180)
            {
                RebuildColorBarCache(range);
            }

            if (colorBarCache != null)
                g.DrawImageUnscaled(colorBarCache, 0, 0);
        }

        private void RebuildColorBarCache(FEMaster.Core.Range range)
        {
            InvalidateColorBarCache();
            colorBarCache = new Bitmap(180, 320);
            colorBarCacheType = ShowResult;
            colorBarCacheWidth = 180;

            using (var g = Graphics.FromImage(colorBarCache))
            {
                g.Clear(Color.Transparent);
                var cmap = new ColorMap(range.Max, range.Min);
                const int x1 = 20, x2 = 40, yTop = 50, yBottom = 300;
                int barH = yBottom - yTop;

                for (int i = 0; i <= 250; i++)
                {
                    double value = range.Min + (range.Span() * i / 250.0);
                    int y = yTop + (int)((1.0 - i / 250.0) * barH);
                    using (var pen = new Pen(cmap.GetColor(value)))
                        g.DrawLine(pen, x1, y, x2, y);
                }

                using (var font = new Font("Arial", 8))
                using (var brush = new SolidBrush(Color.White))
                using (var linePen = new Pen(Color.White))
                {
                    void Tick(int y, double val)
                    {
                        g.DrawLine(linePen, x2, y, x2 + 4, y);
                        g.DrawString(val.ToString("G4"), font, brush, new PointF(x2 + 6, y - 6));
                    }

                    Tick(yTop, range.Max);
                    Tick(yTop + barH / 2, (range.Max + range.Min) / 2);
                    Tick(yBottom, range.Min);
                }
            }
        }

        private void InvalidateColorBarCache()
        {
            if (colorBarCache != null)
            {
                colorBarCache.Dispose();
                colorBarCache = null;
            }
            colorBarCacheType = ResultType.None;
            colorBarCacheWidth = -1;
        }

        private void EnsureGeometryCache(Canvas canvas)
        {
            if (model == null) return;
            if (geometryCacheKey == BuildGeometryCacheKey(canvas)) return;

            int e = model.ElementsNo;
            undeformedScreenCache = new PointF[e][];
            deformedScreenCache = new PointF[e][];
            resultScreenCache = new PointF[e][];
            undeformedBoundsCache = new RectangleF[e];
            deformedBoundsCache = new RectangleF[e];
            resultBoundsCache = new RectangleF[e];
            supportScreenCache = new PointF[model.SupportsNo];
            loadScreenCache = new PointF[model.LoadsNo];

            for (int i = 0; i < e; i++)
            {
                int n1 = model.Elements[i].Node1 - 1;
                int n2 = model.Elements[i].Node2 - 1;
                int n3 = model.Elements[i].Node3 - 1;

                undeformedScreenCache[i] = new[]
                {
                    canvas.WorldToScreen(new Point2D(model.Nodes[n1].X, model.Nodes[n1].Y)),
                    canvas.WorldToScreen(new Point2D(model.Nodes[n2].X, model.Nodes[n2].Y)),
                    canvas.WorldToScreen(new Point2D(model.Nodes[n3].X, model.Nodes[n3].Y))
                };
                deformedScreenCache[i] = new[]
                {
                    canvas.WorldToScreen(new Point2D(model.Nodes[n1].X + model.GetDeformationX(n1) * DeformationZoom,
                                                     model.Nodes[n1].Y + model.GetDeformationY(n1) * DeformationZoom)),
                    canvas.WorldToScreen(new Point2D(model.Nodes[n2].X + model.GetDeformationX(n2) * DeformationZoom,
                                                     model.Nodes[n2].Y + model.GetDeformationY(n2) * DeformationZoom)),
                    canvas.WorldToScreen(new Point2D(model.Nodes[n3].X + model.GetDeformationX(n3) * DeformationZoom,
                                                     model.Nodes[n3].Y + model.GetDeformationY(n3) * DeformationZoom))
                };
                resultScreenCache[i] = deformedScreenCache[i];
                undeformedBoundsCache[i] = BoundsOf(undeformedScreenCache[i]);
                deformedBoundsCache[i] = BoundsOf(deformedScreenCache[i]);
                resultBoundsCache[i] = deformedBoundsCache[i];
            }

            for (int i = 0; i < model.SupportsNo; i++)
            {
                int n = model.Supports[i].NodeNo - 1;
                supportScreenCache[i] = canvas.WorldToScreen(new Point2D(model.Nodes[n].X, model.Nodes[n].Y));
            }

            for (int i = 0; i < model.LoadsNo; i++)
            {
                int n = model.PointLoads[i].NodeNo - 1;
                loadScreenCache[i] = canvas.WorldToScreen(new Point2D(model.Nodes[n].X, model.Nodes[n].Y));
            }

            geometryCacheKey = BuildGeometryCacheKey(canvas);
        }

        private int BuildGeometryCacheKey(Canvas canvas)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + model.NodesNo;
                hash = hash * 31 + model.ElementsNo;
                hash = hash * 31 + canvas.ViewKey;
                hash = hash * 31 + DeformationZoom.GetHashCode();
                return hash;
            }
        }

        private static RectangleF BoundsOf(PointF[] pts)
        {
            float minX = pts[0].X, maxX = pts[0].X;
            float minY = pts[0].Y, maxY = pts[0].Y;
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }
            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }
    }
}

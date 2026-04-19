using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using FEMaster.Form.Drawing.Entities;

namespace FEMaster.Form.Drawing
{
    public class Canvas : IDisposable
    {
        #region Members

        private readonly Graphics graphics;
        private readonly Stack<PipelineState> pipelineStates = new Stack<PipelineState>();
        private readonly Stack<GraphicsState> graphicsStates = new Stack<GraphicsState>();

        #endregion

        #region Functions

        public Func<Point2D, PointF> WorldToScreen;

        public Func<PointF, Point2D> ScreenToWorld;

        #endregion

        #region Properties

        public float ScaleX => graphics.DpiX / 96.0f;

        public float ScaleY => graphics.DpiY / 96.0f;

        #endregion

        #region Constructors

        public Canvas(Graphics graphics)
        {
            this.graphics = graphics;
        }

        #endregion

        public void SaveState()
        {
            graphicsStates.Push(graphics.Save());
        }

        public void RestoreState()
        {
            if (graphicsStates.Count > 0)
            {
                graphics.Restore(graphicsStates.Pop());
            }
        }

        public void GraphicsPipeline(TargetSpace targetSpace, int zIndex = 0, Action<Graphics> context = null)
        {
            pipelineStates.Push(new PipelineState(targetSpace, zIndex, context));
        }

        public void Present()
        {
            var groups = pipelineStates.GroupBy(p => p.Target).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var traits = group.OrderBy(p => p.ZIndex);

                foreach (var trait in traits)
                {
                    trait.Render?.Invoke(graphics);
                }
            }
        }

        public void DrawLine(Line line, DrawableTraits traits)
        {
            var p0 = WorldToScreen(line.P0);
            var p1 = WorldToScreen(line.P1);

            using (var pen = traits.CreatePen())
            {
                graphics.DrawLine(pen, p0, p1);
            }
        }

        public void FillTriangle(Point2D w0, Point2D w1, Point2D w2, Color fill)
        {
            var pts = new[] { WorldToScreen(w0), WorldToScreen(w1), WorldToScreen(w2) };
            using (var brush = new SolidBrush(fill))
                graphics.FillPolygon(brush, pts);
        }

        public void DrawTriangle(Point2D w0, Point2D w1, Point2D w2, Color stroke, float thickness = 1f)
        {
            var pts = new[] { WorldToScreen(w0), WorldToScreen(w1), WorldToScreen(w2) };
            using (var pen = new Pen(stroke, thickness))
                graphics.DrawPolygon(pen, pts);
        }

        public void Dispose()
        {
            graphics?.Dispose();
        }
    }
}
using System;

namespace FEMaster.Form.Drawing.Entities
{
    public class Line : Entity
    {
        #region Properties

        public Point2D P0 { get; set; }

        public Point2D P1 { get; set; }

        public int ZIndex { get; set; }

        public override Extents2D WorldBounds => ComputeWorldBounds();

        #endregion

        #region Constructors

        public Line()
        { }

        #endregion

        public override void Draw(Canvas canvas)
        {
            canvas.GraphicsPipeline(TargetSpace.WorldSpace, ZIndex, context =>
            {
                canvas.DrawLine(this, DrawableTraits);
            });
        }

        protected override Extents2D ComputeWorldBounds(int margin = 5)
        {
            var bounds = Extents2D.FromLTRB(
                (int)Math.Min(P0.X, P1.X), (int)Math.Min(P0.Y, P1.Y),
                (int)Math.Max(P0.X, P1.X), (int)Math.Max(P0.Y, P1.Y));

            return new Extents2D(bounds.X - margin, bounds.Y - margin, bounds.Width + 2.0 * margin, bounds.Height + 2.0 * margin);
        }
    }
}
using System.Drawing;
using System.Windows.Forms;
using FEMaster.Form.Contracts;

namespace FEMaster.Form.Drawing
{
    public class Crosshair : Drawable, IMouseListener
    {
        #region Members

        private Rectangle bounds;

        #endregion

        #region Properties

        public int X { get; private set; }

        public int Y { get; private set; }

        #endregion

        public override void Draw(Canvas canvas)
        {
            canvas.GraphicsPipeline(TargetSpace.ScreenSpaceOverlay, 0, context =>
            {
                context.DrawLine(Pens.White, new Point(X, Y - 40), new Point(X, Y + 40)); // vertical line
                context.DrawLine(Pens.White, new Point(X - 40, Y), new Point(X + 40, Y)); // horizontal line
                context.DrawRectangle(Pens.White, X - 4, Y - 4, 8, 8);
            });
        }

        #region Interface IMouseListener

        public void OnMouseMove(MouseEventArgs args)
        {
            var previous = bounds;
            X = args.X;
            Y = args.Y;
            bounds = new Rectangle(X - 48, Y - 48, 96, 96);
            if (previous.Width > 0 && previous.Height > 0)
                Invalidate(Rectangle.Union(previous, bounds));
            else
                Invalidate(bounds);
        }

        public void OnMouseWheel(MouseEventArgs args)
        { }

        public void OnMouseButtonUp(MouseEventArgs args)
        { }

        public void OnMouseButtonDown(MouseEventArgs args)
        { }

        #endregion
    }
}

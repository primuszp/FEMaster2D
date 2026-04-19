using System.Drawing;

namespace FEMaster.Form.Drawing.Extensions
{
    public static class CanvasExtensions
    {
        public static Extents2D ToWorld(this Canvas canvas, Rectangle bounds)
        {
            var p1 = canvas.ScreenToWorld(bounds.Location);
            var p2 = canvas.ScreenToWorld(new Point2D(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            return Extents2D.FromLTRB(p1.X, p2.Y, p2.X, p1.Y);
        }

        public static Rectangle ToScreen(this Canvas canvas, Extents2D bounds)
        {
            var p1 = canvas.WorldToScreen(bounds.Location);
            var p2 = canvas.WorldToScreen(new Point2D(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            return Rectangle.FromLTRB((int)p1.X, (int)p2.Y, (int)p2.X, (int)p1.Y);
        }
    }
}
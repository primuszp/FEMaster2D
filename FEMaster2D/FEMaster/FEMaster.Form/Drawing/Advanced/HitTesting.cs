using System;
using System.Drawing;

namespace FEMaster.Form.Drawing
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/ms969920.aspx
    /// </summary>
    public static class HitTesting
    {
        public static bool HitTestLine(PointF pt1, PointF pt2, PointF mouse, float width)
        {
            var halfWidth = width / 2.0;

            Point2D a = new Point2D(mouse.X - pt1.X, mouse.Y - pt1.Y);
            Point2D b = new Point2D(pt2.X - pt1.X, pt2.Y - pt1.Y);

            var dist = GetLengthOfNormal(a, b);

            return dist >= -halfWidth && dist <= halfWidth;
        }

        private static double GetLengthOfNormal(Point2D a, Point2D b)
        {
            var c = Point2D.Empty;

            double bb = DotProduct(b, b);
            if (bb < double.Epsilon)
                return VectorMagnitude(a);

            c.X = b.X * (DotProduct(a, b) / bb);
            c.Y = b.Y * (DotProduct(a, b) / bb);

            //
            // Obtain perpendicular projection: e = a - c
            //

            return VectorMagnitude(SubtractVectors(a, c));
        }

        private static double DotProduct(Point2D a, Point2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private static Point2D SubtractVectors(Point2D a, Point2D b)
        {
            return new Point2D
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        private static double VectorMagnitude(Point2D pt)
        {
            return Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
        }
    }
}
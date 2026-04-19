using System;
using System.Drawing;
using System.Globalization;

namespace FEMaster.Form.Drawing
{
    public struct Point2D
    {
        #region Members

        public static readonly Point2D Empty = new Point2D();

        #endregion

        #region Properties

        public double X { get; set; }

        public double Y { get; set; }

        public bool IsEmpty => Math.Abs(X) < double.Epsilon && Math.Abs(Y) < double.Epsilon;

        #endregion

        #region Constructors

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point2D(SizeF sz) :
            this(sz.Width, sz.Height)
        { }

        #endregion

        public static implicit operator PointF(Point2D pt)
        {
            return new PointF((float)pt.X, (float)pt.Y);
        }

        public static explicit operator SizeF(Point2D pt)
        {
            return new SizeF((float)pt.X, (float)pt.Y);
        }

        public static Point2D operator +(Point2D pt, Size sz)
        {
            return Add(pt, sz);
        }

        public static Point2D operator -(Point2D pt, Size sz)
        {
            return Subtract(pt, sz);
        }

        public static Point2D operator +(Point2D pt, SizeF sz)
        {
            return Add(pt, sz);
        }

        public static Point2D operator -(Point2D pt, SizeF sz)
        {
            return Subtract(pt, sz);
        }

        public static bool operator ==(Point2D left, Point2D right)
        {
            return Math.Abs(left.X - right.X) < double.Epsilon && Math.Abs(left.Y - right.Y) < double.Epsilon;
        }

        public static bool operator !=(Point2D left, Point2D right)
        {
            return !(left == right);
        }

        public static Point2D Add(Point2D pt, Size sz)
        {
            return new Point2D(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static Point2D Subtract(Point2D pt, Size sz)
        {
            return new Point2D(pt.X - sz.Width, pt.Y - sz.Height);
        }

        public static Point2D Add(Point2D pt, SizeF sz)
        {
            return new Point2D(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static Point2D Subtract(Point2D pt, SizeF sz)
        {
            return new Point2D(pt.X - sz.Width, pt.Y - sz.Height);
        }

        public static Point Ceiling(Point2D pt)
        {
            return new Point((int)Math.Ceiling(pt.X), (int)Math.Ceiling(pt.Y));
        }

        public static Point Truncate(Point2D pt)
        {
            return new Point((int)pt.X, (int)pt.Y);
        }

        public static Point Round(Point2D pt)
        {
            return new Point((int)Math.Round(pt.X), (int)Math.Round(pt.Y));
        }

        public void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }

        public void Offset(Point2D pt)
        {
            Offset(pt.X, pt.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point2D point)
            {
                return Math.Abs(point.X - X) < double.Epsilon && Math.Abs(point.Y - Y) < double.Epsilon && point.GetType() == GetType();
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", X, Y);
        }
    }
}
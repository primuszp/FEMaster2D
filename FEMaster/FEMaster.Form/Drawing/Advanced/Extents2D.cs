using System;
using System.Drawing;
using System.Globalization;

namespace FEMaster.Form.Drawing
{
    public struct Extents2D
    {
        #region Members

        public static readonly Extents2D Empty = new Extents2D();

        #endregion

        #region Properties

        public double X { get; private set; }

        public double Y { get; private set; }

        public Point2D Location
        {
            get => new Point2D(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public double Left => X;

        public double Top => Y;

        public double Right => X + Width;

        public double Bottom => Y + Height;

        public bool IsEmpty
        {
            get
            {
                if (Width > 0d)
                {
                    return Height <= 0d;
                }
                return true;
            }
        }

        #endregion

        #region Constructors

        public Extents2D(Point2D point, double width, double height)
        {
            X = point.X;
            Y = point.Y;
            Width = width;
            Height = height;
        }

        public Extents2D(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        #endregion

        public static implicit operator Rectangle(Extents2D box)
        {
            return new Rectangle((int)box.X, (int)box.Y, (int)box.Width, (int)box.Height);
        }

        public bool Contains(double x, double y)
        {
            return X <= x && x < X + Width && Y <= y && y < Y + Height;
        }

        public bool Contains(Point2D point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(Extents2D box)
        {
            return X <= box.X && box.X + box.Width <= X + Width && Y <= box.Y && box.Y + box.Height <= Y + Height;
        }

        public void Inflate(double x, double y)
        {
            X -= x;
            Y -= y;
            Width += 2.0 * x;
            Height += 2.0 * y;
        }

        public static Extents2D Inflate(Extents2D box, double x, double y)
        {
            var rect = new Extents2D(box.X, box.Y, box.Width, box.Height);
            rect.Inflate(x, y);
            return rect;
        }

        public void Intersect(Extents2D box)
        {
            var rect = Intersect(box, this);
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }

        public static Extents2D Intersect(Extents2D a, Extents2D b)
        {
            var x = Math.Max(a.X, b.X);
            var w = Math.Min(a.X + a.Width, b.X + b.Width);

            var y = Math.Max(a.Y, b.Y);
            var h = Math.Min(a.Y + a.Height, b.Y + b.Height);

            if (w >= x && h >= y)
            {
                return new Extents2D(x, y, w - x, h - y);
            }
            return new Extents2D();
        }

        public bool IntersectsWith(Extents2D box)
        {
            return box.X < X + Width && X < box.X + box.Width && box.Y < Y + Height && Y < box.Y + box.Height;
        }

        public static Extents2D Union(Extents2D a, Extents2D b)
        {
            var x = Math.Min(a.X, b.X);
            var w = Math.Max(a.X + a.Width, b.X + b.Width);

            var y = Math.Min(a.Y, b.Y);
            var h = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new Extents2D(x, y, w - x, h - y);
        }

        public void Offset(Point point)
        {
            Offset(point.X, point.Y);
        }

        public void Offset(double x, double y)
        {
            X += x;
            Y += y;
        }

        public static Extents2D FromLTRB(double left, double top, double right, double bottom)
        {
            return new Extents2D(left, top, right - left, bottom - top);
        }

        public static bool operator ==(Extents2D left, Extents2D right)
        {
            return Math.Abs(left.X - right.X) <
                   double.Epsilon && Math.Abs(left.Y - right.Y) <
                   double.Epsilon && Math.Abs(left.Width - right.Width) <
                   double.Epsilon && Math.Abs(left.Height - right.Height) < double.Epsilon;
        }

        public static bool operator !=(Extents2D left, Extents2D right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ", Y=" + Y.ToString(CultureInfo.CurrentCulture) + ", Width=" + Width.ToString(CultureInfo.CurrentCulture) + ", Height=" + Height.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Extents2D bb)
            {
                return Math.Abs(bb.X - X) < double.Epsilon &&
                       Math.Abs(bb.Y - Y) < double.Epsilon &&
                       Math.Abs(bb.Width - Width) < double.Epsilon &&
                       Math.Abs(bb.Height - Height) < double.Epsilon;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int)(((uint)X) ^
                                  (((uint)Y << 13) | ((uint)Y >> 19)) ^
                                  (((uint)Width << 26) | ((uint)Width >>  6)) ^
                                  (((uint)Height << 7) | ((uint)Height >> 25))));
        }
    }
}
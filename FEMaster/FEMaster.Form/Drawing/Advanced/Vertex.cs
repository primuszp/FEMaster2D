using System;

namespace FEMaster.Form.Drawing
{
    // A T típus csak értéktípus lehet, és meg kell hogy valósítsa a felsorolt interfészeket, ami fennáll a numerikus
    // típusok esetén például a System.Int32 (int) és a System.Double (double) megvalósítják az alábbi interfészeket
    public class Vertex<T> where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public T X { get; set; }

        public T Y { get; set; }

        public Vertex()
        {
            X = Y = default(T);
        }

        public Vertex(T x, T y)
        {
            X = x;
            Y = y;
        }

        public static Vertex<T> operator +(Vertex<T> p1, Vertex<T> p2)
        {
            return new Vertex<T>((dynamic)p1.X + p2.X, (dynamic)p1.Y + p2.Y);
        }

        public static Vertex<T> operator -(Vertex<T> p1, Vertex<T> p2)
        {
            return new Vertex<T>((dynamic)p1.X - p2.X, (dynamic)p1.Y - p2.Y);
        }
    }
}
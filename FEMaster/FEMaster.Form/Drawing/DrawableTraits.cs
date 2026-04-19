using System.Drawing;
using System.Drawing.Drawing2D;

namespace FEMaster.Form.Drawing
{
    public class DrawableTraits
    {
        #region Properties

        public Color Color { get; internal set; }

        public Color FillColor { get; internal set; }

        public float Thickness { get; internal set; }

        public DashStyle DashStyle { get; internal set; }

        #endregion

        #region Constructors

        public DrawableTraits()
        {
            Color = Color.White;
            Thickness = 1.5000f;
            DashStyle = DashStyle.Solid;
        }

        #endregion

        public Pen CreatePen(float scale = 1.0f)
        {
            return new Pen(Color, Thickness * scale) { DashStyle = DashStyle };
        }

        public static DrawableTraits FromAttributes(Color color, float thickness = 1.0f, DashStyle dashStyle = DashStyle.Solid, Color fillColor = new Color())
        {
            return new DrawableTraits
            {
                Color = color,
                Thickness = thickness,
                FillColor = fillColor,
                DashStyle = dashStyle
            };
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using FEMaster.Form.Drawing;

namespace FEMaster.Form.Controls
{
    public sealed partial class Viewport : UserControl
    {
        #region Members

        private double tx, ty;
        private double zoom = 1;
        private PointF mouseLocation;

        #endregion

        #region Properties

        public double Zoom
        {
            get => zoom;
            set => zoom = value;
        }

        public double TranslateX
        {
            get => tx;
            set => tx = value;
        }

        public double TranslateY
        {
            get => ty;
            set => ty = value;
        }

        public Point2D WorldPosition { get; private set; }

        #endregion

        #region Constructors

        public Viewport()
        {
            InitializeComponent();
            Cursor = Cursors.Cross;
            BackColor = Color.FromArgb(40, 40, 35);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        #endregion

        #region Mouse Events

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            {
                mouseLocation = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            {
                if (e.Button == MouseButtons.Right)
                {
                    tx += mouseLocation.X - e.X;
                    ty += mouseLocation.Y - e.Y;

                    Invalidate();
                }
                mouseLocation = new PointF(e.X, e.Y);
                WorldPosition = ScreenToWorld(mouseLocation);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            var before = ScreenToWorld(e.Location);

            double previousZoom = zoom;
            if (e.Delta > 0)
                SetZoom(true);
            else if (e.Delta < 0)
                SetZoom(false);

            if (Math.Abs(previousZoom - zoom) < double.Epsilon)
                return;

            // Keep the world point under the mouse stable while zooming.
            tx = before.X * zoom - e.X;
            ty = (Height - before.Y) * zoom - e.Y;

            Invalidate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Projects a 2D point from object space into screen space. 
        /// </summary>
        public PointF WorldToScreen(Point2D point)
        {
            return new PointF((float)(point.X * zoom - tx), (float)((Height - point.Y) * zoom - ty));
        }

        /// <summary>
        /// Converts a screen space point (x, y) into a corresponding point in world space.
        /// </summary>
        public Point2D ScreenToWorld(PointF point)
        {
            return ScreenToWorld(point.X, point.Y);
        }

        /// <summary>
        /// Converts a screen space point (x, y) into a corresponding point in world space.
        /// </summary>
        public Point2D ScreenToWorld(double x, double y)
        {
            return new Point2D((x + tx) / zoom, (Height * zoom - y - ty) / zoom);
        }

        public void ZoomIn()
        {
            SetZoom(true);
        }

        public void ZoomOut()
        {
            SetZoom(false);
        }

        private double SetZoom(bool zoomIn, double delta = 0.1d)
        {
            double scale = zoomIn ? Zoom * (1.0d + delta) : Zoom * (1.0d - delta);

            if (0.0005 <= scale && scale <= 2000)
            {
                Zoom = scale;
            }
            return Zoom;
        }

        #endregion
    }
}

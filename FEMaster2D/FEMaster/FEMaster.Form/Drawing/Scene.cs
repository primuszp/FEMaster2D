using System;
using System.Drawing;
using System.Windows.Forms;
using FEMaster.Form.Controls;
using FEMaster.Form.Contracts;
using FEMaster.Form.Drawing.Entities;

namespace FEMaster.Form.Drawing
{
    public sealed class Scene : EntityGroup
    {
        #region Members

        private readonly Viewport viewport;

        #endregion

        #region Constructors

        public Scene(Viewport vp)
        {
            viewport = vp;
            viewport.Paint += OnViewportPaint;
            viewport.Resize += OnViewportResize;
            viewport.MouseUp += OnViewportMouseUp;
            viewport.MouseMove += OnViewportMouseMove;
            viewport.MouseDown += OnViewportMouseDown;
            viewport.MouseWheel += OnViewportMouseWheel;
            viewport.MouseEnter += (s, e) => Cursor.Hide();
            viewport.MouseLeave += (s, e) => Cursor.Show();
        }

        #endregion

        #region Viewport Events

        private void OnViewportPaint(object sender, PaintEventArgs e)
        {
            Draw(new Canvas(e.Graphics)
            {
                WorldToScreen = viewport.WorldToScreen,
                ScreenToWorld = viewport.ScreenToWorld
            });
        }

        private void OnViewportResize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnViewportMouseUp(object sender, MouseEventArgs e)
        {
            ForEach(drawable =>
            {
                if (drawable is IMouseListener listener)
                {
                    listener.OnMouseButtonUp(e);
                }
            });
        }

        private void OnViewportMouseMove(object sender, MouseEventArgs e)
        {
            ForEach(drawable =>
            {
                if (drawable is IMouseListener listener)
                {
                    listener.OnMouseMove(e);
                }
            });
        }

        private void OnViewportMouseDown(object sender, MouseEventArgs e)
        {
            ForEach(drawable =>
            {
                if (drawable is IMouseListener listener)
                {
                    listener.OnMouseButtonDown(e);
                }


                if (drawable is Line line && e.Button == MouseButtons.Left)
                {
                    var pt1 = viewport.WorldToScreen(line.P0);
                    var pt2 = viewport.WorldToScreen(line.P1);

                    if (HitTesting.HitTestLine(pt1, pt2, e.Location, 2))
                    {
                        line.IsSelected = !line.IsSelected;
                    }
                }

            });
        }

        private void OnViewportMouseWheel(object sender, MouseEventArgs e)
        {
            ForEach(drawable =>
            {
                if (drawable is IMouseListener listener)
                {
                    listener.OnMouseWheel(e);
                }
            });
        }

        #endregion

        #region Public Methods

        public override void Draw(Canvas canvas)
        {
            ForEach(drawable =>
            {
                drawable.Draw(canvas);
            });
            canvas.Present();
        }

        public override void ForEach(Action<Drawable> action)
        {
            foreach (var child in this)
            {
                child.ForEach(action);
            }
        }

        public override void Invalidate()
        {
            viewport?.Invalidate();
        }

        public override void Invalidate(Rectangle rectangle)
        {
            viewport?.Invalidate(rectangle);
        }

        #endregion
    }
}
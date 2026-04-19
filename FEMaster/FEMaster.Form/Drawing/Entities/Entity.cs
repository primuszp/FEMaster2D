using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FEMaster.Form.Drawing.Entities
{
    public abstract class Entity : Drawable
    {
        #region Members

        private Color color;
        private float thickness;

        private bool selected;
        private bool highlighted;

        #endregion

        #region Properties

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                UpdateDrawableStates();
            }
        }

        public float Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                UpdateDrawableStates();
            }
        }

        public virtual bool IsSelected
        {
            get => selected;
            internal set
            {
                if (value != selected)
                {
                    selected = value;
                    UpdateDrawableStates();
                }
            }
        }

        public virtual bool IsHighlighted
        {
            get => highlighted;
            internal set
            {
                if (value != highlighted)
                {
                    highlighted = value;
                    UpdateDrawableStates();
                }
            }
        }

        /// <summary>
        /// Accesses the world bounds of the drawable.
        /// </summary>
        public virtual Extents2D WorldBounds => ComputeWorldBounds();

        #endregion

        protected Entity()
        {
            Color = Color.White;
            Thickness = 1.5000f;
        }

        private void UpdateDrawableStates()
        {
            var traits = IsSelected ?
                DrawableTraits.FromAttributes(Color, Thickness * 2f, DashStyle.Dash) :
                DrawableTraits.FromAttributes(Color, Thickness / 2f);

            SetAttributes(traits);
            Invalidate();
        }

        protected virtual Extents2D ComputeWorldBounds(int margin = 5)
        {
            return Extents2D.Empty;
        }
    }
}
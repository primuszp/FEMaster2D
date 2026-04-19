using System;
using System.Drawing;

namespace FEMaster.Form.Drawing
{
    public abstract class Drawable
    {
        #region Properties

        public Guid Id { get; internal set; }

        public Drawable Parent { get; internal set; }

        protected DrawableTraits DrawableTraits { get; private set; }

        #endregion

        #region Constructors

        protected Drawable()
        {
            Id = Guid.NewGuid();
            DrawableTraits = new DrawableTraits();
        }

        #endregion

        #region Methods

        public virtual void Draw(Canvas canvas)
        {
            canvas.GraphicsPipeline(TargetSpace.WorldSpace);
        }

        public virtual void SetAttributes(DrawableTraits traits)
        {
            DrawableTraits = traits;
        }

        public virtual void Invalidate()
        {
            Parent?.Invalidate();
        }

        public virtual void Invalidate(Rectangle rectangle)
        {
            Parent?.Invalidate(rectangle);
        }

        public virtual void ForEach(Action<Drawable> action)
        {
            action(this);
        }

        #endregion
    }
}
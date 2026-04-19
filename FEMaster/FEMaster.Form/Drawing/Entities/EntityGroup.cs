using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using FEMaster.Form.Drawing.Extensions;

namespace FEMaster.Form.Drawing.Entities
{
    public class EntityGroup : Entity, IEnumerable<Drawable>
    {
        #region Members

        private readonly IList<Drawable> children = new List<Drawable>();

        #endregion

        #region Properties

        public bool HasChildren => children.Count > 0;

        public override Extents2D WorldBounds => ComputeWorldBounds();

        #endregion

        public override void Draw(Canvas canvas)
        {
            canvas.GraphicsPipeline(TargetSpace.WorldSpace, 0, graphics =>
            {
                var rect = canvas.ToScreen(WorldBounds);
                graphics.DrawRectangle(Pens.Bisque, rect);
            });
        }

        public override void ForEach(Action<Drawable> action)
        {
            action(this);

            foreach (var child in this)
            {
                child.ForEach(action);
            }
        }

        public virtual void Attach(Drawable drawable)
        {
            drawable.Parent = this;
            children.Add(drawable);
        }

        public virtual void Detach(Drawable drawable)
        {
            drawable.Parent = null;
            children.Remove(drawable);
        }

        public virtual Drawable GetDrawable(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

        public virtual void Clear()
        {
            children.Clear();
        }

        protected override Extents2D ComputeWorldBounds(int margin = 5)
        {
            var bounds = Extents2D.Empty;

            foreach (var child in this)
            {
                if (child is Entity entity)
                {
                    bounds = bounds == Extents2D.Empty ?
                        entity.WorldBounds : Extents2D.Union(bounds, entity.WorldBounds);
                }
            }

            return new Extents2D(bounds.X - margin, bounds.Y - margin, bounds.Width + 2.0 * margin, bounds.Height + 2.0 * margin);
        }

        #region From IEnumerable Interface

        public IEnumerator<Drawable> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
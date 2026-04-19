using System;
using System.Collections;
using System.Collections.Generic;

namespace FEMaster.Form.Drawing
{
    public abstract class DrawableGroup : Drawable, IEnumerable<Drawable>
    {
        #region Members

        private readonly IList<Drawable> children = new List<Drawable>();

        #endregion

        #region Properties

        public bool HasChildren => children.Count > 0;

        #endregion

        #region Methods

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            {
                foreach (var child in this)
                {
                    child.Draw(canvas);
                }
            }
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

        #endregion

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
using System;
using System.Drawing;

namespace FEMaster.Form.Drawing
{
    internal sealed class PipelineState : IComparable
    {
        #region Properties

        public int ZIndex { get; }

        public TargetSpace Target { get; }

        public Action<Graphics> Render { get; }

        #endregion

        #region Constructor

        public PipelineState(TargetSpace target, int zIndex = 0, Action<Graphics> render = null)
        {
            Target = target;
            ZIndex = zIndex;
            Render = render;
        }

        #endregion

        #region From IComparable interface

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case PipelineState pt when ZIndex == pt.ZIndex:
                    return 0;
                case PipelineState pt when ZIndex < pt.ZIndex:
                    return -1;
                case PipelineState pt when ZIndex > pt.ZIndex:
                    return +1;
            }
            throw new ArgumentException($"Object is not a {nameof(PipelineState)}");
        }

        #endregion
    }
}
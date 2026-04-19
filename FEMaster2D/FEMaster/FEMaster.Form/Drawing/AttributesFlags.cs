using System;

namespace FEMaster.Form.Drawing
{
    [Flags]
    public enum AttributesFlags
    {
        DrawableIsNormal = 0,
        DrawableIsVisible = 1,
        DrawableIsSelected = 2,
        DrawableIsHighlighted = 4
    }
}
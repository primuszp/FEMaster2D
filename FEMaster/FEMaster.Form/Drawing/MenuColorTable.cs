using System.Drawing;
using System.Windows.Forms;

namespace FEMaster.Form.Drawing
{
    public class MenuColorTable : ProfessionalColorTable
    {
        private static readonly Color Hover = Color.FromArgb(210, 210, 215);

        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd   => Hover;
        public override Color MenuItemSelected              => Hover;
        public override Color MenuItemBorder                => Color.FromArgb(160, 160, 170);
        public override Color MenuItemPressedGradientBegin  => Color.FromArgb(190, 190, 200);
        public override Color MenuItemPressedGradientEnd    => Color.FromArgb(190, 190, 200);
        public override Color MenuStripGradientBegin        => SystemColors.MenuBar;
        public override Color MenuStripGradientEnd          => SystemColors.MenuBar;
        public override Color ToolStripDropDownBackground   => SystemColors.Menu;
        public override Color ImageMarginGradientBegin      => SystemColors.Menu;
        public override Color ImageMarginGradientMiddle     => SystemColors.Menu;
        public override Color ImageMarginGradientEnd        => SystemColors.Menu;
    }

    // Forces text to always be black regardless of item state,
    // which prevents the "invisible until hover" rendering bug.
    public class FlatMenuRenderer : ToolStripProfessionalRenderer
    {
        public FlatMenuRenderer() : base(new MenuColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.Black;
            base.OnRenderItemText(e);
        }
    }
}

using System;
using System.Drawing;

namespace FEMaster.Form.Drawing
{
    /// <summary>
    /// A simple implementation of colormap, only red-blue map is implemented.
    /// </summary>
    public class ColorMap
    {
        #region Members

        private Color[] colors;
        private int segments = 250;

        #endregion

        #region Properties

        public double Max { get; set; }
        public double Min { get; set; }

        #endregion

        #region Contructors

        public ColorMap(double max, double min)
        {
            Max = max;
            Min = min;
            SetColors();
        }

        #endregion

        public Color GetColor(double value)
        {
            var range = Max - Min;
            if (colors == null || colors.Length == 0) return Color.Blue;
            if (Math.Abs(range) < double.Epsilon) return colors[colors.Length / 2];
            var ratio = (value - Min) / range;
            var index = (int)Math.Round(ratio * segments);
            index = Math.Max(0, Math.Min(segments, index));
            return colors[index];
        }

        private void SetColors()
        {
            colors = new Color[segments + 1];
            Color[] mainColors = { Color.Blue, Color.Cyan, Color.LightGreen, Color.Yellow, Color.Red };

            var percentStep = 100d / (mainColors.Length - 1);
            var indices = new int[mainColors.Length]; // we will have these many main indices

            for (var i = 0; i < mainColors.Length; i++)
                indices[i] = Convert.ToInt32(i * percentStep / 100 * segments);

            indices[0] = 0;
            indices[indices.Length - 1] = segments;
            colors = new Color[segments + 1];

            for (var i = 0; i < indices.Length - 1; i++)
            {
                var span = Math.Max(1, indices[i + 1] - indices[i]);
                var c = GetSteppedColors(mainColors[i + 1], mainColors[i], span);

                for (var j = 0; j < c.Length; j++)
                {
                    colors[indices[i] + j] = c[j];
                }
            }
        }

        private Color[] GetSteppedColors(Color cTop, Color cBottom, int nPoints)
        {
            Color btm = cBottom;
            Color top = cTop;
            Color[] cols = new Color[nPoints + 1];

            var dr = Convert.ToInt32(top.R) - Convert.ToInt32(btm.R);
            var dg = Convert.ToInt32(top.G) - btm.G;
            var db = Convert.ToInt32(top.B) - btm.B;

            // These differences have to be spanned in the nPoints-1 steps

            var stepR = dr / (double)nPoints;
            var stepG = dg / (double)nPoints;
            var stepB = db / (double)nPoints;

            for (int i = 0; i <= nPoints; i++)
            {
                int r = Convert.ToInt32(btm.R + i * stepR);
                int g = Convert.ToInt32(btm.G + i * stepG);
                int b = Convert.ToInt32(btm.B + i * stepB);

                cols[i] = Color.FromArgb(r, g, b);
            }
            return cols;
        }

    }
}

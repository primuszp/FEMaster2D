using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using FEMaster.Core;
using FEMaster.Form.Drawing;

namespace FEMaster.Form
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private Scene scene;
        private Model model;
        private MeshEntity meshEntity;
        private Crosshair crosshair;

        public MainForm()
        {
            InitializeComponent();

            // Set here, not in Designer, so VS reformatting cannot strip it.
            menuStrip.Renderer = new FEMaster.Form.Drawing.FlatMenuRenderer();

            scene = new Scene(viewport);
            crosshair = new Crosshair();
            scene.Attach(crosshair);
        }

        #region File Menu

        private void OnSaveImageAsClick(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Save Image As";
                dialog.Filter = "PNG Files|*.png|JPEG Files|*.jpg|BMP Files|*.bmp|TIFF Files|*.tiff|All Files|*.*";
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog() != DialogResult.OK) return;

                using (var bmp = new System.Drawing.Bitmap(viewport.Width, viewport.Height))
                {
                    viewport.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, viewport.Width, viewport.Height));

                    ImageFormat format = ImageFormat.Bmp;
                    switch (dialog.FilterIndex)
                    {
                        case 1: format = ImageFormat.Png; break;
                        case 2: format = ImageFormat.Jpeg; break;
                        case 3: format = ImageFormat.Bmp; break;
                        case 4: format = ImageFormat.Tiff; break;
                    }

                    bmp.Save(dialog.FileName, format);
                }
            }
        }

        private void OnOpenClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Open FEM file";
                dialog.Filter = "FEM Files (*.fem)|*.fem|All Files|*.*";

                if (dialog.ShowDialog() != DialogResult.OK) return;

                model = Model.Load(File.OpenRead(dialog.FileName), ex =>
                    MessageBox.Show(ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error));

                if (model == null || model.ElementsNo == 0)
                {
                    MessageBox.Show("Failed to load model or model is empty.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                analyseMenuItem.Enabled = true;
                refineMeshMenuItem.Enabled = true;
                showReportMenuItem.Enabled = false;

                RebuildScene();
                FitToView();
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "FEMaster 2D",
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
                Application.Exit();
        }

        #endregion

        #region Analyse Menu

        private async void OnAnalyseClick(object sender, EventArgs e)
        {
            if (model == null) return;

            SetAnalysisRunning(true);
            try
            {
                var m = model;
                await Task.Run(() => m.Analyse());
                meshEntity?.AutoScaleDeformationZoom();
                showReportMenuItem.Enabled = true;
                scene.Invalidate();
                statusLabel.Text = $"Analysis complete - {model.ElementsNo} elements, {model.NodesNo} nodes";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Analysis failed";
                MessageBox.Show(ex.Message, "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetAnalysisRunning(false);
            }
        }

        private void SetAnalysisRunning(bool running)
        {
            analyseMenuItem.Enabled            = !running;
            refineMeshMenuItem.Enabled         = !running;
            progressBar.Visible                = running;
            progressBar.MarqueeAnimationSpeed  = running ? 30 : 0;
            statusLabel.Text                   = running ? "Analysing..." : "Ready";
            Cursor                             = running ? Cursors.WaitCursor : Cursors.Default;
            statusStrip.Refresh();
        }

        private void OnAnalyseMenuOpening(object sender, EventArgs e)
        {
            var isDirect = model == null || model.BcMethod == FEMaster.Core.BoundaryConditionMethod.DirectCondensation;
            bcDirectMenuItem.Checked  =  isDirect;
            bcPenaltyMenuItem.Checked = !isDirect;
        }

        private void OnBcMethodClick(object sender, EventArgs e)
        {
            if (model == null) return;
            model.BcMethod = sender == bcDirectMenuItem
                ? FEMaster.Core.BoundaryConditionMethod.DirectCondensation
                : FEMaster.Core.BoundaryConditionMethod.Penalty;
        }

        private void OnRefineMeshClick(object sender, EventArgs e)
        {
            if (model == null) return;
            model = MeshRefiner.Refine(model);
            showReportMenuItem.Enabled = false;
            RebuildScene();
            FitToView();
        }

        private void OnDefZoomClick(object sender, EventArgs e)
        {
            if (meshEntity == null) return;

            var current = meshEntity.DeformationZoom.ToString("G");
            var input = ShowInputDialog("Set deformation zoom factor:", "Deformation Zoom", current);

            if (string.IsNullOrEmpty(input)) return;

            if (double.TryParse(input, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                meshEntity.DeformationZoom = val;
                scene.Invalidate();
            }
        }

        #endregion

        #region Show Menu

        private void OnShowMenuOpening(object sender, EventArgs e)
        {
            if (meshEntity == null) return;

            showModelMenuItem.Checked = meshEntity.ShowUndeformed;
            showDeformedMenuItem.Checked = meshEntity.ShowDeformed;
            showNodeNumbersMenuItem.Checked = meshEntity.ShowNodeNumbers;
            showEdgesMenuItem.Checked = meshEntity.ShowElementEdges;

            resultNoneMenuItem.Checked      = meshEntity.ShowResult == ResultType.None;
            resultSigmaXMenuItem.Checked    = meshEntity.ShowResult == ResultType.SigmaX;
            resultSigmaYMenuItem.Checked    = meshEntity.ShowResult == ResultType.SigmaY;
            resultTauXYMenuItem.Checked     = meshEntity.ShowResult == ResultType.TauXY;
            resultEpsilonXMenuItem.Checked  = meshEntity.ShowResult == ResultType.EpsilonX;
            resultEpsilonYMenuItem.Checked  = meshEntity.ShowResult == ResultType.EpsilonY;
            resultGammaXYMenuItem.Checked   = meshEntity.ShowResult == ResultType.GammaXY;
        }

        private void OnShowResultClick(object sender, EventArgs e)
        {
            if (meshEntity == null) return;

            if (sender == showModelMenuItem)
                meshEntity.ShowUndeformed = !meshEntity.ShowUndeformed;
            else if (sender == showDeformedMenuItem)
                meshEntity.ShowDeformed = !meshEntity.ShowDeformed;
            else if (sender == showNodeNumbersMenuItem)
                meshEntity.ShowNodeNumbers = !meshEntity.ShowNodeNumbers;
            else if (sender == showEdgesMenuItem)
                meshEntity.ShowElementEdges = !meshEntity.ShowElementEdges;
            else if (sender == resultNoneMenuItem)
                meshEntity.ShowResult = ResultType.None;
            else if (sender == resultSigmaXMenuItem)
                meshEntity.ShowResult = ResultType.SigmaX;
            else if (sender == resultSigmaYMenuItem)
                meshEntity.ShowResult = ResultType.SigmaY;
            else if (sender == resultTauXYMenuItem)
                meshEntity.ShowResult = ResultType.TauXY;
            else if (sender == resultEpsilonXMenuItem)
                meshEntity.ShowResult = ResultType.EpsilonX;
            else if (sender == resultEpsilonYMenuItem)
                meshEntity.ShowResult = ResultType.EpsilonY;
            else if (sender == resultGammaXYMenuItem)
                meshEntity.ShowResult = ResultType.GammaXY;

            scene.Invalidate();
        }

        #endregion

        #region Results Menu

        private void OnShowReportClick(object sender, EventArgs e)
        {
            if (model == null || !model.HasResults) return;

            using (var form = new System.Windows.Forms.Form())
            {
                form.Text = "Analysis Report";
                form.Size = new System.Drawing.Size(700, 500);
                form.StartPosition = FormStartPosition.CenterParent;

                var textBox = new System.Windows.Forms.TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    ReadOnly = true,
                    Font = new System.Drawing.Font("Courier New", 9),
                    Text = model.Report()
                };

                form.Controls.Add(textBox);
                form.ShowDialog(this);
            }
        }

        #endregion

        #region Private Helpers

        private void RebuildScene()
        {
            scene.Clear();
            scene.Attach(crosshair);

            meshEntity = new MeshEntity(model);
            meshEntity.ElementSelected += OnElementSelected;
            scene.Attach(meshEntity);
            statusLabel.Text = "Ready";
        }

        private void OnElementSelected(object sender, FEMaster.Form.Drawing.ElementSelectedEventArgs e)
        {
            if (e.ElementIndex < 0)
            {
                statusLabel.Text = "Ready";
                return;
            }

            var el = model.Elements[e.ElementIndex];
            string info = $"Element {el.ElementNo}   Nodes: {el.Node1}, {el.Node2}, {el.Node3}";

            if (model.HasResults)
            {
                info += $"   σx={el.Stresses[0]:G4}   σy={el.Stresses[1]:G4}" +
                        $"   τxy={el.Stresses[2]:G4}" +
                        $"   εx={el.Strains[0]:G4}   εy={el.Strains[1]:G4}   γxy={el.Strains[2]:G4}";
            }

            statusLabel.Text = info;
        }

        private void FitToView()
        {
            if (model == null || model.NodesNo == 0) return;

            double xMin = double.MaxValue, xMax = double.MinValue;
            double yMin = double.MaxValue, yMax = double.MinValue;

            for (int i = 0; i < model.NodesNo; i++)
            {
                if (model.Nodes[i].X < xMin) xMin = model.Nodes[i].X;
                if (model.Nodes[i].X > xMax) xMax = model.Nodes[i].X;
                if (model.Nodes[i].Y < yMin) yMin = model.Nodes[i].Y;
                if (model.Nodes[i].Y > yMax) yMax = model.Nodes[i].Y;
            }

            double w = xMax - xMin;
            double h = yMax - yMin;
            if (w < double.Epsilon || h < double.Epsilon) return;

            const double margin = 0.1;
            double vpW = viewport.Width;
            double vpH = viewport.Height;

            double scaleX = vpW * (1 - 2 * margin) / w;
            double scaleY = vpH * (1 - 2 * margin) / h;
            double scale = Math.Min(scaleX, scaleY);

            // Center of model in world coords
            double xMid = (xMin + xMax) / 2.0;
            double yMid = (yMin + yMax) / 2.0;

            // WorldToScreen: screen_x = x*zoom - tx,  screen_y = (vpH - y)*zoom - ty
            // We want center of model to map to center of viewport:
            //   xMid*zoom - tx = vpW/2  →  tx = xMid*zoom - vpW/2
            //   (vpH - yMid)*zoom - ty = vpH/2  →  ty = (vpH - yMid)*zoom - vpH/2
            viewport.Zoom = scale;
            viewport.TranslateX = xMid * scale - vpW / 2.0;
            viewport.TranslateY = (vpH - yMid) * scale - vpH / 2.0;

            scene.Invalidate();
        }

        private static string ShowInputDialog(string prompt, string title, string defaultValue)
        {
            using (var form = new System.Windows.Forms.Form())
            {
                form.Text = title;
                form.Size = new System.Drawing.Size(320, 120);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                var label = new Label { Left = 10, Top = 10, Width = 280, Text = prompt };
                var textBox = new TextBox { Left = 10, Top = 30, Width = 280, Text = defaultValue };
                var ok = new Button { Text = "OK", Left = 130, Width = 75, Top = 55, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 215, Width = 75, Top = 55, DialogResult = DialogResult.Cancel };

                form.Controls.AddRange(new Control[] { label, textBox, ok, cancel });
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
            }
        }

        #endregion
    }
}

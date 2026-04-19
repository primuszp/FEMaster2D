namespace FEMaster.Form
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.analyseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.bcDirectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bcPenaltyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.defZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.refineMeshMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.showModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDeformedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNodeNumbersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEdgesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.resultNoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultSigmaXMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultSigmaYMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultTauXYMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultEpsilonXMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultEpsilonYMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultGammaXYMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resultsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.showReportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewport = new FEMaster.Form.Controls.Viewport();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuStrip.ForeColor = System.Drawing.Color.Black;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.analyseMenu,
            this.showMenu,
            this.resultsMenu});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1000, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.toolStripSeparator1,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "&File";
            // 
            // openMenuItem
            // 
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openMenuItem.Size = new System.Drawing.Size(155, 22);
            this.openMenuItem.Text = "&Open...";
            this.openMenuItem.Click += new System.EventHandler(this.OnOpenClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.OnExitClick);
            // 
            // analyseMenu
            // 
            this.analyseMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analyseMenuItem,
            this.toolStripSeparator2,
            this.bcDirectMenuItem,
            this.bcPenaltyMenuItem,
            this.toolStripSeparator4,
            this.defZoomMenuItem,
            this.toolStripSeparator5,
            this.refineMeshMenuItem});
            this.analyseMenu.Name = "analyseMenu";
            this.analyseMenu.Size = new System.Drawing.Size(60, 20);
            this.analyseMenu.Text = "&Analyse";
            this.analyseMenu.DropDownOpening += new System.EventHandler(this.OnAnalyseMenuOpening);
            // 
            // analyseMenuItem
            // 
            this.analyseMenuItem.Enabled = false;
            this.analyseMenuItem.Name = "analyseMenuItem";
            this.analyseMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.analyseMenuItem.Size = new System.Drawing.Size(292, 22);
            this.analyseMenuItem.Text = "&Run Analysis";
            this.analyseMenuItem.Click += new System.EventHandler(this.OnAnalyseClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(289, 6);
            // 
            // bcDirectMenuItem
            // 
            this.bcDirectMenuItem.Name = "bcDirectMenuItem";
            this.bcDirectMenuItem.Size = new System.Drawing.Size(292, 22);
            this.bcDirectMenuItem.Text = "BC: &Direct Condensation (recommended)";
            this.bcDirectMenuItem.Click += new System.EventHandler(this.OnBcMethodClick);
            // 
            // bcPenaltyMenuItem
            // 
            this.bcPenaltyMenuItem.Name = "bcPenaltyMenuItem";
            this.bcPenaltyMenuItem.Size = new System.Drawing.Size(292, 22);
            this.bcPenaltyMenuItem.Text = "BC: &Penalty Method";
            this.bcPenaltyMenuItem.Click += new System.EventHandler(this.OnBcMethodClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(289, 6);
            // 
            // defZoomMenuItem
            // 
            this.defZoomMenuItem.Name = "defZoomMenuItem";
            this.defZoomMenuItem.Size = new System.Drawing.Size(292, 22);
            this.defZoomMenuItem.Text = "Set Deformation &Zoom...";
            this.defZoomMenuItem.Click += new System.EventHandler(this.OnDefZoomClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(289, 6);
            // 
            // refineMeshMenuItem
            // 
            this.refineMeshMenuItem.Enabled = false;
            this.refineMeshMenuItem.Name = "refineMeshMenuItem";
            this.refineMeshMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.refineMeshMenuItem.Size = new System.Drawing.Size(292, 22);
            this.refineMeshMenuItem.Text = "Re&fine Mesh (1→4)";
            this.refineMeshMenuItem.Click += new System.EventHandler(this.OnRefineMeshClick);
            // 
            // showMenu
            // 
            this.showMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showModelMenuItem,
            this.showDeformedMenuItem,
            this.showNodeNumbersMenuItem,
            this.showEdgesMenuItem,
            this.toolStripSeparator3,
            this.resultNoneMenuItem,
            this.resultSigmaXMenuItem,
            this.resultSigmaYMenuItem,
            this.resultTauXYMenuItem,
            this.resultEpsilonXMenuItem,
            this.resultEpsilonYMenuItem,
            this.resultGammaXYMenuItem});
            this.showMenu.Name = "showMenu";
            this.showMenu.Size = new System.Drawing.Size(48, 20);
            this.showMenu.Text = "&Show";
            this.showMenu.DropDownOpening += new System.EventHandler(this.OnShowMenuOpening);
            // 
            // showModelMenuItem
            // 
            this.showModelMenuItem.Checked = true;
            this.showModelMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showModelMenuItem.Name = "showModelMenuItem";
            this.showModelMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showModelMenuItem.Text = "Undeformed &Model";
            this.showModelMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // showDeformedMenuItem
            // 
            this.showDeformedMenuItem.Checked = true;
            this.showDeformedMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDeformedMenuItem.Name = "showDeformedMenuItem";
            this.showDeformedMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showDeformedMenuItem.Text = "&Deformed Shape";
            this.showDeformedMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // showNodeNumbersMenuItem
            // 
            this.showNodeNumbersMenuItem.Name = "showNodeNumbersMenuItem";
            this.showNodeNumbersMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showNodeNumbersMenuItem.Text = "&Node Numbers";
            this.showNodeNumbersMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // showEdgesMenuItem
            // 
            this.showEdgesMenuItem.Checked = true;
            this.showEdgesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showEdgesMenuItem.Name = "showEdgesMenuItem";
            this.showEdgesMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showEdgesMenuItem.Text = "Element &Edges on Deformed";
            this.showEdgesMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(221, 6);
            // 
            // resultNoneMenuItem
            // 
            this.resultNoneMenuItem.Name = "resultNoneMenuItem";
            this.resultNoneMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultNoneMenuItem.Text = "&None";
            this.resultNoneMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultSigmaXMenuItem
            // 
            this.resultSigmaXMenuItem.Name = "resultSigmaXMenuItem";
            this.resultSigmaXMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultSigmaXMenuItem.Text = "σ&x  (SigmaX)";
            this.resultSigmaXMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultSigmaYMenuItem
            // 
            this.resultSigmaYMenuItem.Name = "resultSigmaYMenuItem";
            this.resultSigmaYMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultSigmaYMenuItem.Text = "σ&y  (SigmaY)";
            this.resultSigmaYMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultTauXYMenuItem
            // 
            this.resultTauXYMenuItem.Name = "resultTauXYMenuItem";
            this.resultTauXYMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultTauXYMenuItem.Text = "τ&xy (TauXY)";
            this.resultTauXYMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultEpsilonXMenuItem
            // 
            this.resultEpsilonXMenuItem.Name = "resultEpsilonXMenuItem";
            this.resultEpsilonXMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultEpsilonXMenuItem.Text = "ε&x  (EpsilonX)";
            this.resultEpsilonXMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultEpsilonYMenuItem
            // 
            this.resultEpsilonYMenuItem.Name = "resultEpsilonYMenuItem";
            this.resultEpsilonYMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultEpsilonYMenuItem.Text = "ε&y  (EpsilonY)";
            this.resultEpsilonYMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultGammaXYMenuItem
            // 
            this.resultGammaXYMenuItem.Name = "resultGammaXYMenuItem";
            this.resultGammaXYMenuItem.Size = new System.Drawing.Size(224, 22);
            this.resultGammaXYMenuItem.Text = "γ&xy (GammaXY)";
            this.resultGammaXYMenuItem.Click += new System.EventHandler(this.OnShowResultClick);
            // 
            // resultsMenu
            // 
            this.resultsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showReportMenuItem});
            this.resultsMenu.Name = "resultsMenu";
            this.resultsMenu.Size = new System.Drawing.Size(56, 20);
            this.resultsMenu.Text = "&Results";
            // 
            // showReportMenuItem
            // 
            this.showReportMenuItem.Enabled = false;
            this.showReportMenuItem.Name = "showReportMenuItem";
            this.showReportMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.showReportMenuItem.Size = new System.Drawing.Size(191, 22);
            this.showReportMenuItem.Text = "&Show Report...";
            this.showReportMenuItem.Click += new System.EventHandler(this.OnShowReportClick);
            // 
            // viewport
            // 
            this.viewport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(35)))));
            this.viewport.Cursor = System.Windows.Forms.Cursors.Cross;
            this.viewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewport.Location = new System.Drawing.Point(0, 24);
            this.viewport.Name = "viewport";
            this.viewport.Size = new System.Drawing.Size(1000, 554);
            this.viewport.TabIndex = 0;
            this.viewport.TranslateX = 0D;
            this.viewport.TranslateY = 0D;
            this.viewport.Zoom = 1D;
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.statusStrip.ForeColor = System.Drawing.Color.White;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // statusLabel
            // 
            this.statusLabel.ForeColor = System.Drawing.Color.White;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(852, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Ready";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(180, 16);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 0;
            this.progressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.viewport);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "FEMaster 2D — Finite Element Solver";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.Viewport viewport;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analyseMenu;
        private System.Windows.Forms.ToolStripMenuItem analyseMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem bcDirectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bcPenaltyMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem defZoomMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem refineMeshMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMenu;
        private System.Windows.Forms.ToolStripMenuItem showModelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDeformedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showNodeNumbersMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEdgesMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem resultNoneMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultSigmaXMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultSigmaYMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultTauXYMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultEpsilonXMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultEpsilonYMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultGammaXYMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultsMenu;
        private System.Windows.Forms.ToolStripMenuItem showReportMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
    }
}

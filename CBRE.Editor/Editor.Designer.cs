using CBRE.Editor.UI;

namespace CBRE.Editor {
    partial class Editor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			CBRE.Editor.UI.TableSplitConfiguration tableSplitConfiguration1 = new CBRE.Editor.UI.TableSplitConfiguration();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
			this.mnuMain = new System.Windows.Forms.MenuStrip();
			this.stsStatus = new System.Windows.Forms.StatusStrip();
			this.StatusTextLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusSelectionLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusCoordinatesLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusBoxLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusZoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusSnapLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tscToolStrip = new System.Windows.Forms.ToolStripContainer();
			this.DockFill = new CBRE.Editor.UI.DockedPanel();
			this.TableSplitView = new CBRE.Editor.UI.TableSplitControl();
			this.DocumentTabs = new CBRE.Editor.UI.ClosableTabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.tabPage7 = new System.Windows.Forms.TabPage();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.RightSidebar = new CBRE.Editor.UI.DockedPanel();
			this.tspTools = new System.Windows.Forms.ToolStrip();
			this.stsStatus.SuspendLayout();
			this.tscToolStrip.ContentPanel.SuspendLayout();
			this.tscToolStrip.LeftToolStripPanel.SuspendLayout();
			this.tscToolStrip.SuspendLayout();
			this.DockFill.SuspendLayout();
			this.DocumentTabs.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnuMain
			// 
			this.mnuMain.Location = new System.Drawing.Point(0, 0);
			this.mnuMain.Name = "mnuMain";
			this.mnuMain.Size = new System.Drawing.Size(879, 24);
			this.mnuMain.TabIndex = 0;
			this.mnuMain.Text = "menuStrip1";
			// 
			// stsStatus
			// 
			this.stsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusTextLabel,
            this.StatusSelectionLabel,
            this.StatusCoordinatesLabel,
            this.StatusBoxLabel,
            this.StatusZoomLabel,
            this.StatusSnapLabel});
			this.stsStatus.Location = new System.Drawing.Point(0, 726);
			this.stsStatus.Name = "stsStatus";
			this.stsStatus.Size = new System.Drawing.Size(879, 24);
			this.stsStatus.TabIndex = 1;
			this.stsStatus.Text = "statusStrip1";
			// 
			// StatusTextLabel
			// 
			this.StatusTextLabel.Name = "StatusTextLabel";
			this.StatusTextLabel.Size = new System.Drawing.Size(314, 19);
			this.StatusTextLabel.Spring = true;
			this.StatusTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// StatusSelectionLabel
			// 
			this.StatusSelectionLabel.AutoSize = false;
			this.StatusSelectionLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.StatusSelectionLabel.Name = "StatusSelectionLabel";
			this.StatusSelectionLabel.Size = new System.Drawing.Size(130, 19);
			// 
			// StatusCoordinatesLabel
			// 
			this.StatusCoordinatesLabel.AutoSize = false;
			this.StatusCoordinatesLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.StatusCoordinatesLabel.Name = "StatusCoordinatesLabel";
			this.StatusCoordinatesLabel.Size = new System.Drawing.Size(130, 19);
			// 
			// StatusBoxLabel
			// 
			this.StatusBoxLabel.AutoSize = false;
			this.StatusBoxLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.StatusBoxLabel.Name = "StatusBoxLabel";
			this.StatusBoxLabel.Size = new System.Drawing.Size(130, 19);
			// 
			// StatusZoomLabel
			// 
			this.StatusZoomLabel.AutoSize = false;
			this.StatusZoomLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.StatusZoomLabel.Name = "StatusZoomLabel";
			this.StatusZoomLabel.Size = new System.Drawing.Size(80, 19);
			// 
			// StatusSnapLabel
			// 
			this.StatusSnapLabel.AutoSize = false;
			this.StatusSnapLabel.Name = "StatusSnapLabel";
			this.StatusSnapLabel.Size = new System.Drawing.Size(80, 19);
			// 
			// tscToolStrip
			// 
			// 
			// tscToolStrip.ContentPanel
			// 
			this.tscToolStrip.ContentPanel.Controls.Add(this.DockFill);
			this.tscToolStrip.ContentPanel.Controls.Add(this.RightSidebar);
			this.tscToolStrip.ContentPanel.Size = new System.Drawing.Size(845, 677);
			this.tscToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// tscToolStrip.LeftToolStripPanel
			// 
			this.tscToolStrip.LeftToolStripPanel.Controls.Add(this.tspTools);
			this.tscToolStrip.LeftToolStripPanel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tscToolStrip.Location = new System.Drawing.Point(0, 24);
			this.tscToolStrip.Name = "tscToolStrip";
			this.tscToolStrip.Size = new System.Drawing.Size(879, 702);
			this.tscToolStrip.TabIndex = 2;
			this.tscToolStrip.Text = "tscToolStrip";
			// 
			// tscToolStrip.TopToolStripPanel
			// 
			this.tscToolStrip.TopToolStripPanel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// DockFill
			// 
			this.DockFill.Controls.Add(this.TableSplitView);
			this.DockFill.Controls.Add(this.DocumentTabs);
			this.DockFill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DockFill.DockDimension = 0;
			this.DockFill.Hidden = false;
			this.DockFill.Location = new System.Drawing.Point(0, 0);
			this.DockFill.Name = "DockFill";
			this.DockFill.Size = new System.Drawing.Size(601, 677);
			this.DockFill.TabIndex = 4;
			// 
			// TableSplitView
			// 
			this.TableSplitView.ColumnCount = 2;
			this.TableSplitView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tableSplitConfiguration1.Columns = 2;
			tableSplitConfiguration1.Rectangles = ((System.Collections.Generic.List<System.Drawing.Rectangle>)(resources.GetObject("tableSplitConfiguration1.Rectangles")));
			tableSplitConfiguration1.Rows = 2;
			this.TableSplitView.Configuration = tableSplitConfiguration1;
			this.TableSplitView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableSplitView.Location = new System.Drawing.Point(0, 24);
			this.TableSplitView.MinimumViewSize = 2;
			this.TableSplitView.Name = "TableSplitView";
			this.TableSplitView.RowCount = 2;
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableSplitView.Size = new System.Drawing.Size(601, 653);
			this.TableSplitView.TabIndex = 0;
			// 
			// DocumentTabs
			// 
			this.DocumentTabs.Controls.Add(this.tabPage2);
			this.DocumentTabs.Controls.Add(this.tabPage3);
			this.DocumentTabs.Controls.Add(this.tabPage4);
			this.DocumentTabs.Controls.Add(this.tabPage5);
			this.DocumentTabs.Controls.Add(this.tabPage6);
			this.DocumentTabs.Controls.Add(this.tabPage7);
			this.DocumentTabs.Controls.Add(this.tabPage1);
			this.DocumentTabs.Dock = System.Windows.Forms.DockStyle.Top;
			this.DocumentTabs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DocumentTabs.Location = new System.Drawing.Point(0, 0);
			this.DocumentTabs.Name = "DocumentTabs";
			this.DocumentTabs.SelectedIndex = 0;
			this.DocumentTabs.Size = new System.Drawing.Size(601, 24);
			this.DocumentTabs.TabIndex = 2;
			this.DocumentTabs.RequestClose += new CBRE.Editor.UI.ClosableTabControl.RequestCloseEventHandler(this.DocumentTabsRequestClose);
			this.DocumentTabs.SelectedIndexChanged += new System.EventHandler(this.DocumentTabsSelectedIndexChanged);
			// 
			// tabPage2
			// 
			this.tabPage2.ImageIndex = 0;
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(593, -5);
			this.tabPage2.TabIndex = 0;
			this.tabPage2.Text = "Test 1";
			this.tabPage2.Visible = false;
			// 
			// tabPage3
			// 
			this.tabPage3.ImageIndex = 0;
			this.tabPage3.Location = new System.Drawing.Point(4, 25);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(593, -5);
			this.tabPage3.TabIndex = 1;
			this.tabPage3.Text = "Test 2";
			this.tabPage3.Visible = false;
			// 
			// tabPage4
			// 
			this.tabPage4.ImageIndex = 0;
			this.tabPage4.Location = new System.Drawing.Point(4, 25);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(593, -5);
			this.tabPage4.TabIndex = 2;
			this.tabPage4.Text = "Test 3";
			this.tabPage4.Visible = false;
			// 
			// tabPage5
			// 
			this.tabPage5.ImageIndex = 0;
			this.tabPage5.Location = new System.Drawing.Point(4, 25);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(593, -5);
			this.tabPage5.TabIndex = 3;
			this.tabPage5.Text = "Test 4";
			this.tabPage5.Visible = false;
			// 
			// tabPage6
			// 
			this.tabPage6.ImageIndex = 0;
			this.tabPage6.Location = new System.Drawing.Point(4, 25);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Size = new System.Drawing.Size(593, -5);
			this.tabPage6.TabIndex = 4;
			this.tabPage6.Text = "Test 5";
			this.tabPage6.Visible = false;
			// 
			// tabPage7
			// 
			this.tabPage7.ImageIndex = 0;
			this.tabPage7.Location = new System.Drawing.Point(4, 25);
			this.tabPage7.Name = "tabPage7";
			this.tabPage7.Size = new System.Drawing.Size(593, -5);
			this.tabPage7.TabIndex = 5;
			this.tabPage7.Text = "Test 6";
			this.tabPage7.Visible = false;
			// 
			// tabPage1
			// 
			this.tabPage1.ImageIndex = 0;
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(593, -5);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Test 7";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// RightSidebar
			// 
			this.RightSidebar.Dock = System.Windows.Forms.DockStyle.Right;
			this.RightSidebar.DockDimension = 244;
			this.RightSidebar.Hidden = false;
			this.RightSidebar.Location = new System.Drawing.Point(601, 0);
			this.RightSidebar.MinSize = 230;
			this.RightSidebar.Name = "RightSidebar";
			this.RightSidebar.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.RightSidebar.Size = new System.Drawing.Size(244, 677);
			this.RightSidebar.TabIndex = 3;
			// 
			// tspTools
			// 
			this.tspTools.Dock = System.Windows.Forms.DockStyle.None;
			this.tspTools.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.tspTools.Location = new System.Drawing.Point(0, 3);
			this.tspTools.Name = "tspTools";
			this.tspTools.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.tspTools.Size = new System.Drawing.Size(34, 117);
			this.tspTools.TabIndex = 0;
			// 
			// Editor
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(879, 750);
			this.Controls.Add(this.tscToolStrip);
			this.Controls.Add(this.stsStatus);
			this.Controls.Add(this.mnuMain);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.mnuMain;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "Editor";
			this.Text = "CBRE-EX";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorClosing);
			this.Load += new System.EventHandler(this.EditorLoad);
			this.Shown += new System.EventHandler(this.EditorShown);
			this.stsStatus.ResumeLayout(false);
			this.stsStatus.PerformLayout();
			this.tscToolStrip.ContentPanel.ResumeLayout(false);
			this.tscToolStrip.LeftToolStripPanel.ResumeLayout(false);
			this.tscToolStrip.LeftToolStripPanel.PerformLayout();
			this.tscToolStrip.ResumeLayout(false);
			this.tscToolStrip.PerformLayout();
			this.DockFill.ResumeLayout(false);
			this.DocumentTabs.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.StatusStrip stsStatus;
        private System.Windows.Forms.ToolStripContainer tscToolStrip;
        private System.Windows.Forms.ToolStrip tspTools;
        private UI.TableSplitControl TableSplitView;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusSelectionLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusCoordinatesLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusBoxLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusZoomLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusSnapLabel;
        public UI.ClosableTabControl DocumentTabs;
        private System.Windows.Forms.TabPage tabPage1;
        private UI.DockedPanel DockFill;
        private UI.DockedPanel RightSidebar;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.TabPage tabPage7;
	}
}


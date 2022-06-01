namespace CBRE.Editor.UI
{
    partial class MapInformationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.NumSolids = new System.Windows.Forms.Label();
			this.NumFaces = new System.Windows.Forms.Label();
			this.NumPointEntities = new System.Windows.Forms.Label();
			this.NumSolidEntities = new System.Windows.Forms.Label();
			this.NumUniqueTextures = new System.Windows.Forms.Label();
			this.TextureMemory = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.TexturePackages = new System.Windows.Forms.ListBox();
			this.CloseButton = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.NumSolids, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.NumFaces, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.NumPointEntities, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.NumSolidEntities, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.NumUniqueTextures, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.TextureMemory, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(282, 120);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(102, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Solids:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(3, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(102, 20);
			this.label2.TabIndex = 0;
			this.label2.Text = "Faces:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(3, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 20);
			this.label3.TabIndex = 0;
			this.label3.Text = "Point Entities:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(3, 60);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(102, 20);
			this.label4.TabIndex = 0;
			this.label4.Text = "Solid Entities:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(3, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(102, 20);
			this.label5.TabIndex = 0;
			this.label5.Text = "Unique Textures:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(3, 100);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(102, 20);
			this.label6.TabIndex = 0;
			this.label6.Text = "Texture Memory:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// NumSolids
			// 
			this.NumSolids.Location = new System.Drawing.Point(111, 0);
			this.NumSolids.Name = "NumSolids";
			this.NumSolids.Size = new System.Drawing.Size(168, 20);
			this.NumSolids.TabIndex = 0;
			this.NumSolids.Text = "12345";
			this.NumSolids.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// NumFaces
			// 
			this.NumFaces.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumFaces.Location = new System.Drawing.Point(111, 20);
			this.NumFaces.Name = "NumFaces";
			this.NumFaces.Size = new System.Drawing.Size(168, 20);
			this.NumFaces.TabIndex = 0;
			this.NumFaces.Text = "12345";
			this.NumFaces.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// NumPointEntities
			// 
			this.NumPointEntities.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumPointEntities.Location = new System.Drawing.Point(111, 40);
			this.NumPointEntities.Name = "NumPointEntities";
			this.NumPointEntities.Size = new System.Drawing.Size(168, 20);
			this.NumPointEntities.TabIndex = 0;
			this.NumPointEntities.Text = "12345";
			this.NumPointEntities.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// NumSolidEntities
			// 
			this.NumSolidEntities.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumSolidEntities.Location = new System.Drawing.Point(111, 60);
			this.NumSolidEntities.Name = "NumSolidEntities";
			this.NumSolidEntities.Size = new System.Drawing.Size(168, 20);
			this.NumSolidEntities.TabIndex = 0;
			this.NumSolidEntities.Text = "12345";
			this.NumSolidEntities.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// NumUniqueTextures
			// 
			this.NumUniqueTextures.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumUniqueTextures.Location = new System.Drawing.Point(111, 80);
			this.NumUniqueTextures.Name = "NumUniqueTextures";
			this.NumUniqueTextures.Size = new System.Drawing.Size(168, 20);
			this.NumUniqueTextures.TabIndex = 0;
			this.NumUniqueTextures.Text = "12345";
			this.NumUniqueTextures.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TextureMemory
			// 
			this.TextureMemory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextureMemory.Location = new System.Drawing.Point(111, 100);
			this.TextureMemory.Name = "TextureMemory";
			this.TextureMemory.Size = new System.Drawing.Size(168, 20);
			this.TextureMemory.TabIndex = 0;
			this.TextureMemory.Text = "12345";
			this.TextureMemory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(9, 152);
			this.label7.Margin = new System.Windows.Forms.Padding(3);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(128, 15);
			this.label7.TabIndex = 1;
			this.label7.Text = "Texture packages used:";
			// 
			// TexturePackages
			// 
			this.TexturePackages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TexturePackages.FormattingEnabled = true;
			this.TexturePackages.ItemHeight = 15;
			this.TexturePackages.Location = new System.Drawing.Point(12, 173);
			this.TexturePackages.Name = "TexturePackages";
			this.TexturePackages.Size = new System.Drawing.Size(282, 109);
			this.TexturePackages.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Location = new System.Drawing.Point(219, 288);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label8.Location = new System.Drawing.Point(18, 144);
			this.label8.Margin = new System.Windows.Forms.Padding(9);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(270, 2);
			this.label8.TabIndex = 14;
			// 
			// MapInformationDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(306, 322);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.TexturePackages);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapInformationDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Map Information";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label NumSolids;
        private System.Windows.Forms.Label NumFaces;
        private System.Windows.Forms.Label NumPointEntities;
        private System.Windows.Forms.Label NumSolidEntities;
        private System.Windows.Forms.Label NumUniqueTextures;
        private System.Windows.Forms.Label TextureMemory;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox TexturePackages;
        private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Label label8;
	}
}
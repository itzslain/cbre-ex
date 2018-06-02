namespace Sledge.Editor.Compiling
{
    partial class RM2ExportForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textureDims = new System.Windows.Forms.TextBox();
            this.downscaleFactor = new System.Windows.Forms.TextBox();
            this.blurRadius = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ambientRed = new System.Windows.Forms.TextBox();
            this.ambientGreen = new System.Windows.Forms.TextBox();
            this.ambientBlue = new System.Windows.Forms.TextBox();
            this.export = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.ambientColorBox = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Lightmap settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Texture dimensions";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Downscale factor";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Blur radius";
            // 
            // textureDims
            // 
            this.textureDims.Location = new System.Drawing.Point(121, 45);
            this.textureDims.Name = "textureDims";
            this.textureDims.Size = new System.Drawing.Size(179, 20);
            this.textureDims.TabIndex = 4;
            this.textureDims.Text = "2048";
            this.textureDims.LostFocus += new System.EventHandler(this.textureDims_LostFocus);
            // 
            // downscaleFactor
            // 
            this.downscaleFactor.Location = new System.Drawing.Point(121, 68);
            this.downscaleFactor.Name = "downscaleFactor";
            this.downscaleFactor.Size = new System.Drawing.Size(179, 20);
            this.downscaleFactor.TabIndex = 5;
            this.downscaleFactor.Text = "10";
            this.downscaleFactor.LostFocus += new System.EventHandler(this.downscaleFactor_LostFocus);
            // 
            // blurRadius
            // 
            this.blurRadius.Location = new System.Drawing.Point(121, 92);
            this.blurRadius.Name = "blurRadius";
            this.blurRadius.Size = new System.Drawing.Size(179, 20);
            this.blurRadius.TabIndex = 6;
            this.blurRadius.Text = "2";
            this.blurRadius.LostFocus += new System.EventHandler(this.blurRadius_LostFocus);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Ambient color";
            // 
            // ambientRed
            // 
            this.ambientRed.Location = new System.Drawing.Point(121, 131);
            this.ambientRed.Name = "ambientRed";
            this.ambientRed.Size = new System.Drawing.Size(41, 20);
            this.ambientRed.TabIndex = 8;
            this.ambientRed.Text = Lightmapper.AmbientColor.R.ToString();
            this.ambientRed.TextChanged += new System.EventHandler(this.ambientRed_TextChanged);
            this.ambientRed.LostFocus += new System.EventHandler(this.ambientRed_LostFocus);
            // 
            // ambientGreen
            // 
            this.ambientGreen.Location = new System.Drawing.Point(168, 131);
            this.ambientGreen.Name = "ambientGreen";
            this.ambientGreen.Size = new System.Drawing.Size(41, 20);
            this.ambientGreen.TabIndex = 9;
            this.ambientGreen.Text = Lightmapper.AmbientColor.G.ToString();
            this.ambientGreen.TextChanged += new System.EventHandler(this.ambientGreen_TextChanged);
            this.ambientGreen.LostFocus += new System.EventHandler(this.ambientGreen_LostFocus);
            // 
            // ambientBlue
            // 
            this.ambientBlue.Location = new System.Drawing.Point(215, 131);
            this.ambientBlue.Name = "ambientBlue";
            this.ambientBlue.Size = new System.Drawing.Size(41, 20);
            this.ambientBlue.TabIndex = 10;
            this.ambientBlue.Text = Lightmapper.AmbientColor.B.ToString();
            this.ambientBlue.TextChanged += new System.EventHandler(this.ambientBlue_TextChanged);
            this.ambientBlue.LostFocus += new System.EventHandler(this.ambientBlue_LostFocus);
            // 
            // export
            // 
            this.export.Location = new System.Drawing.Point(317, 168);
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(75, 23);
            this.export.TabIndex = 12;
            this.export.Text = "Export";
            this.export.UseVisualStyleBackColor = true;
            this.export.Click += new System.EventHandler(this.export_Click);
            // 
            // cancel
            // 
            this.cancel.Enabled = false;
            this.cancel.Location = new System.Drawing.Point(317, 197);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 13;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Enabled = false;
            this.ProgressBar.Location = new System.Drawing.Point(16, 168);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(295, 23);
            this.ProgressBar.TabIndex = 14;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Location = new System.Drawing.Point(17, 197);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(16, 13);
            this.ProgressLabel.TabIndex = 15;
            this.ProgressLabel.Text = "...";
            this.ProgressLabel.Click += new System.EventHandler(this.label6_Click);
            // 
            // ambientColorBox
            // 
            this.ambientColorBox.BackColor = Lightmapper.AmbientColor;
            this.ambientColorBox.Location = new System.Drawing.Point(263, 131);
            this.ambientColorBox.Name = "ambientColorBox";
            this.ambientColorBox.Size = new System.Drawing.Size(37, 20);
            this.ambientColorBox.TabIndex = 16;
            this.ambientColorBox.Click += new System.EventHandler(this.ambientColorBox_Click);
            // 
            // RM2ExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 232);
            this.Controls.Add(this.ambientColorBox);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.export);
            this.Controls.Add(this.ambientBlue);
            this.Controls.Add(this.ambientGreen);
            this.Controls.Add(this.ambientRed);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.blurRadius);
            this.Controls.Add(this.downscaleFactor);
            this.Controls.Add(this.textureDims);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(420, 270);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(420, 270);
            this.Name = "RM2ExportForm";
            this.ShowIcon = false;
            this.Text = "RM2ExportForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textureDims;
        private System.Windows.Forms.TextBox downscaleFactor;
        private System.Windows.Forms.TextBox blurRadius;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ambientRed;
        private System.Windows.Forms.TextBox ambientGreen;
        private System.Windows.Forms.TextBox ambientBlue;
        private System.Windows.Forms.Button export;
        private System.Windows.Forms.Button cancel;
        public System.Windows.Forms.ProgressBar ProgressBar;
        public System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.Panel ambientColorBox;
    }
}
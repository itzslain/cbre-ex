namespace CBRE.Editor.Logging
{
    partial class EntityErrorWindow
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
            this.logLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.systemBitmap = new System.Windows.Forms.PictureBox();
            this.copyButton = new System.Windows.Forms.Button();
            this.errorTextBox = new CBRE.UI.ReadOnlyTextBox();
            this.dontShowAgainCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.systemBitmap)).BeginInit();
            this.SuspendLayout();
            // 
            // logLabel
            // 
            this.logLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.logLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logLabel.Location = new System.Drawing.Point(50, 12);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(452, 32);
            this.logLabel.TabIndex = 0;
            this.logLabel.Text = "CBRE-EX has encountered errors loading the custom entities.\r\n";
            this.logLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(427, 446);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // systemBitmap
            // 
            this.systemBitmap.Location = new System.Drawing.Point(12, 12);
            this.systemBitmap.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
            this.systemBitmap.Name = "systemBitmap";
            this.systemBitmap.Size = new System.Drawing.Size(32, 32);
            this.systemBitmap.TabIndex = 3;
            this.systemBitmap.TabStop = false;
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.copyButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.copyButton.Location = new System.Drawing.Point(300, 446);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(121, 23);
            this.copyButton.TabIndex = 1;
            this.copyButton.Text = "Copy To Clipboard";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // errorTextBox
            // 
            this.errorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.errorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.errorTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorTextBox.Location = new System.Drawing.Point(12, 56);
            this.errorTextBox.Multiline = true;
            this.errorTextBox.Name = "errorTextBox";
            this.errorTextBox.ReadOnly = true;
            this.errorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.errorTextBox.Size = new System.Drawing.Size(490, 384);
            this.errorTextBox.TabIndex = 2;
            this.errorTextBox.TabStop = false;
            // 
            // dontShowAgainCheckbox
            // 
            this.dontShowAgainCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dontShowAgainCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dontShowAgainCheckbox.Location = new System.Drawing.Point(12, 447);
            this.dontShowAgainCheckbox.Name = "dontShowAgainCheckbox";
            this.dontShowAgainCheckbox.Size = new System.Drawing.Size(282, 23);
            this.dontShowAgainCheckbox.TabIndex = 4;
            this.dontShowAgainCheckbox.Text = "Don\'t show this window again in this session";
            this.dontShowAgainCheckbox.UseVisualStyleBackColor = true;
            // 
            // EntityErrorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 481);
            this.Controls.Add(this.dontShowAgainCheckbox);
            this.Controls.Add(this.errorTextBox);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.systemBitmap);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.logLabel);
            this.MinimumSize = new System.Drawing.Size(530, 520);
            this.Name = "EntityErrorWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Custom Entity Errors";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.systemBitmap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.CheckBox dontShowAgainCheckbox;

        #endregion

        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.PictureBox systemBitmap;
        private System.Windows.Forms.Button copyButton;
        private CBRE.UI.ReadOnlyTextBox errorTextBox;
    }
}
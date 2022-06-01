namespace CBRE.Editor.Logging {
    partial class ExceptionWindow {
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
			this.label4 = new System.Windows.Forms.Label();
			this.FrameworkVersion = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.OperatingSystem = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.CBREVersion = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.FullError = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.copyButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(12, 36);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(72, 15);
			this.label4.TabIndex = 2;
			this.label4.Text = ".NET Version";
			// 
			// FrameworkVersion
			// 
			this.FrameworkVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FrameworkVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FrameworkVersion.Location = new System.Drawing.Point(135, 33);
			this.FrameworkVersion.Name = "FrameworkVersion";
			this.FrameworkVersion.ReadOnly = true;
			this.FrameworkVersion.Size = new System.Drawing.Size(675, 23);
			this.FrameworkVersion.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 66);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Operating System";
			// 
			// OperatingSystem
			// 
			this.OperatingSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OperatingSystem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OperatingSystem.Location = new System.Drawing.Point(135, 62);
			this.OperatingSystem.Name = "OperatingSystem";
			this.OperatingSystem.ReadOnly = true;
			this.OperatingSystem.Size = new System.Drawing.Size(675, 23);
			this.OperatingSystem.TabIndex = 3;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 95);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(91, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "CBRE-EX Version";
			// 
			// CBREVersion
			// 
			this.CBREVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CBREVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CBREVersion.Location = new System.Drawing.Point(135, 91);
			this.CBREVersion.Name = "CBREVersion";
			this.CBREVersion.ReadOnly = true;
			this.CBREVersion.Size = new System.Drawing.Size(675, 23);
			this.CBREVersion.TabIndex = 3;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 120);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(94, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Full Error Message";
			// 
			// FullError
			// 
			this.FullError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FullError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FullError.Location = new System.Drawing.Point(135, 120);
			this.FullError.Multiline = true;
			this.FullError.Name = "FullError";
			this.FullError.ReadOnly = true;
			this.FullError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FullError.Size = new System.Drawing.Size(675, 347);
			this.FullError.TabIndex = 3;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Location = new System.Drawing.Point(8, 444);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(121, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Close";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.copyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.copyButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyButton.Location = new System.Drawing.Point(8, 415);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(121, 23);
			this.copyButton.TabIndex = 5;
			this.copyButton.Text = "Copy Error";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(12, 12);
			this.label2.Margin = new System.Windows.Forms.Padding(3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(202, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "funny placeholder funny placeholder";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ExceptionWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(822, 479);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.FullError);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.CBREVersion);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.OperatingSystem);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.FrameworkVersion);
			this.Controls.Add(this.label4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 486);
			this.Name = "ExceptionWindow";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Error! Something went horribly wrong.";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FrameworkVersion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox OperatingSystem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox CBREVersion;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox FullError;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Label label2;
    }
}
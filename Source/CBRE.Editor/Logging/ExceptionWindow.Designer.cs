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
        private void InitializeComponent()
        {
	        this.label4 = new System.Windows.Forms.Label();
	        this.label5 = new System.Windows.Forms.Label();
	        this.OperatingSystem = new CBRE.UI.ReadOnlyTextBox();
	        this.label6 = new System.Windows.Forms.Label();
	        this.CBREVersion = new CBRE.UI.ReadOnlyTextBox();
	        this.label7 = new System.Windows.Forms.Label();
	        this.FullError = new CBRE.UI.ReadOnlyTextBox();
	        this.cancelButton = new System.Windows.Forms.Button();
	        this.copyButton = new System.Windows.Forms.Button();
	        this.HeaderLabel = new System.Windows.Forms.Label();
	        this.systemBitmap = new System.Windows.Forms.PictureBox();
	        this.AvailableMemory = new CBRE.UI.ReadOnlyTextBox();
	        this.label1 = new System.Windows.Forms.Label();
	        this.RuntimeVersion = new CBRE.UI.ReadOnlyTextBox();
	        this.label2 = new System.Windows.Forms.Label();
	        this.reportButton = new System.Windows.Forms.Button();
	        this.ProcessorName = new CBRE.UI.ReadOnlyTextBox();
	        ((System.ComponentModel.ISupportInitialize)(this.systemBitmap)).BeginInit();
	        this.SuspendLayout();
	        // 
	        // label4
	        // 
	        this.label4.AutoSize = true;
	        this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label4.Location = new System.Drawing.Point(12, 56);
	        this.label4.Name = "label4";
	        this.label4.Size = new System.Drawing.Size(99, 15);
	        this.label4.TabIndex = 1;
	        this.label4.Text = "System Processor";
	        // 
	        // label5
	        // 
	        this.label5.AutoSize = true;
	        this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label5.Location = new System.Drawing.Point(10, 113);
	        this.label5.Name = "label5";
	        this.label5.Size = new System.Drawing.Size(101, 15);
	        this.label5.TabIndex = 3;
	        this.label5.Text = "Operating System";
	        // 
	        // OperatingSystem
	        // 
	        this.OperatingSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.OperatingSystem.BackColor = System.Drawing.SystemColors.Window;
	        this.OperatingSystem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.OperatingSystem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.OperatingSystem.Location = new System.Drawing.Point(117, 111);
	        this.OperatingSystem.Name = "OperatingSystem";
	        this.OperatingSystem.ReadOnly = true;
	        this.OperatingSystem.Size = new System.Drawing.Size(665, 23);
	        this.OperatingSystem.TabIndex = 5;
	        // 
	        // label6
	        // 
	        this.label6.AutoSize = true;
	        this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label6.Location = new System.Drawing.Point(17, 172);
	        this.label6.Name = "label6";
	        this.label6.Size = new System.Drawing.Size(94, 15);
	        this.label6.TabIndex = 5;
	        this.label6.Text = "CBRE-EX Version";
	        // 
	        // CBREVersion
	        // 
	        this.CBREVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.CBREVersion.BackColor = System.Drawing.SystemColors.Window;
	        this.CBREVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.CBREVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.CBREVersion.Location = new System.Drawing.Point(117, 169);
	        this.CBREVersion.Name = "CBREVersion";
	        this.CBREVersion.ReadOnly = true;
	        this.CBREVersion.Size = new System.Drawing.Size(665, 23);
	        this.CBREVersion.TabIndex = 6;
	        // 
	        // label7
	        // 
	        this.label7.AutoSize = true;
	        this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label7.Location = new System.Drawing.Point(8, 201);
	        this.label7.Name = "label7";
	        this.label7.Size = new System.Drawing.Size(103, 15);
	        this.label7.TabIndex = 7;
	        this.label7.Text = "Full Error Message";
	        // 
	        // FullError
	        // 
	        this.FullError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.FullError.BackColor = System.Drawing.SystemColors.Window;
	        this.FullError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.FullError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.FullError.Location = new System.Drawing.Point(117, 198);
	        this.FullError.Multiline = true;
	        this.FullError.Name = "FullError";
	        this.FullError.ReadOnly = true;
	        this.FullError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
	        this.FullError.Size = new System.Drawing.Size(665, 281);
	        this.FullError.TabIndex = 7;
	        // 
	        // cancelButton
	        // 
	        this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
	        this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
	        this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.cancelButton.Location = new System.Drawing.Point(8, 456);
	        this.cancelButton.Name = "cancelButton";
	        this.cancelButton.Size = new System.Drawing.Size(103, 23);
	        this.cancelButton.TabIndex = 0;
	        this.cancelButton.Text = "Close";
	        this.cancelButton.UseVisualStyleBackColor = true;
	        this.cancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
	        // 
	        // copyButton
	        // 
	        this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
	        this.copyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
	        this.copyButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.copyButton.Location = new System.Drawing.Point(8, 398);
	        this.copyButton.Name = "copyButton";
	        this.copyButton.Size = new System.Drawing.Size(103, 23);
	        this.copyButton.TabIndex = 8;
	        this.copyButton.Text = "Copy Error";
	        this.copyButton.UseVisualStyleBackColor = true;
	        this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
	        // 
	        // HeaderLabel
	        // 
	        this.HeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.HeaderLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.HeaderLabel.Location = new System.Drawing.Point(50, 12);
	        this.HeaderLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
	        this.HeaderLabel.Name = "HeaderLabel";
	        this.HeaderLabel.Size = new System.Drawing.Size(732, 32);
	        this.HeaderLabel.TabIndex = 11;
	        this.HeaderLabel.Text = "CBRE-EX has encountered an error but couldn\'t recover from it. CBRE-EX will try t" + "o continue running. More information below.\r\n";
	        this.HeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	        // 
	        // systemBitmap
	        // 
	        this.systemBitmap.Location = new System.Drawing.Point(12, 12);
	        this.systemBitmap.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
	        this.systemBitmap.Name = "systemBitmap";
	        this.systemBitmap.Size = new System.Drawing.Size(32, 32);
	        this.systemBitmap.TabIndex = 12;
	        this.systemBitmap.TabStop = false;
	        // 
	        // AvailableMemory
	        // 
	        this.AvailableMemory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.AvailableMemory.BackColor = System.Drawing.SystemColors.Window;
	        this.AvailableMemory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.AvailableMemory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.AvailableMemory.Location = new System.Drawing.Point(117, 82);
	        this.AvailableMemory.Name = "AvailableMemory";
	        this.AvailableMemory.ReadOnly = true;
	        this.AvailableMemory.Size = new System.Drawing.Size(665, 23);
	        this.AvailableMemory.TabIndex = 3;
	        // 
	        // label1
	        // 
	        this.label1.AutoSize = true;
	        this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label1.Location = new System.Drawing.Point(8, 85);
	        this.label1.Name = "label1";
	        this.label1.Size = new System.Drawing.Size(103, 15);
	        this.label1.TabIndex = 14;
	        this.label1.Text = "Available Memory";
	        // 
	        // RuntimeVersion
	        // 
	        this.RuntimeVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.RuntimeVersion.BackColor = System.Drawing.SystemColors.Window;
	        this.RuntimeVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.RuntimeVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.RuntimeVersion.Location = new System.Drawing.Point(117, 140);
	        this.RuntimeVersion.Name = "RuntimeVersion";
	        this.RuntimeVersion.ReadOnly = true;
	        this.RuntimeVersion.Size = new System.Drawing.Size(665, 23);
	        this.RuntimeVersion.TabIndex = 4;
	        // 
	        // label2
	        // 
	        this.label2.AutoSize = true;
	        this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.label2.Location = new System.Drawing.Point(39, 142);
	        this.label2.Name = "label2";
	        this.label2.Size = new System.Drawing.Size(72, 15);
	        this.label2.TabIndex = 16;
	        this.label2.Text = ".NET Version";
	        // 
	        // reportButton
	        // 
	        this.reportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
	        this.reportButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
	        this.reportButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.reportButton.Location = new System.Drawing.Point(8, 427);
	        this.reportButton.Name = "reportButton";
	        this.reportButton.Size = new System.Drawing.Size(103, 23);
	        this.reportButton.TabIndex = 9;
	        this.reportButton.Text = "Report Bug...";
	        this.reportButton.UseVisualStyleBackColor = true;
	        this.reportButton.Click += new System.EventHandler(this.reportButton_Click);
	        // 
	        // ProcessorName
	        // 
	        this.ProcessorName.BackColor = System.Drawing.SystemColors.Window;
	        this.ProcessorName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	        this.ProcessorName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	        this.ProcessorName.Location = new System.Drawing.Point(117, 53);
	        this.ProcessorName.Name = "ProcessorName";
	        this.ProcessorName.ReadOnly = true;
	        this.ProcessorName.Size = new System.Drawing.Size(665, 23);
	        this.ProcessorName.TabIndex = 2;
	        // 
	        // ExceptionWindow
	        // 
	        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	        this.ClientSize = new System.Drawing.Size(794, 491);
	        this.Controls.Add(this.ProcessorName);
	        this.Controls.Add(this.reportButton);
	        this.Controls.Add(this.label2);
	        this.Controls.Add(this.RuntimeVersion);
	        this.Controls.Add(this.label1);
	        this.Controls.Add(this.AvailableMemory);
	        this.Controls.Add(this.systemBitmap);
	        this.Controls.Add(this.HeaderLabel);
	        this.Controls.Add(this.copyButton);
	        this.Controls.Add(this.cancelButton);
	        this.Controls.Add(this.FullError);
	        this.Controls.Add(this.label7);
	        this.Controls.Add(this.CBREVersion);
	        this.Controls.Add(this.label6);
	        this.Controls.Add(this.OperatingSystem);
	        this.Controls.Add(this.label5);
	        this.Controls.Add(this.label4);
	        this.MaximizeBox = false;
	        this.MinimizeBox = false;
	        this.MinimumSize = new System.Drawing.Size(810, 530);
	        this.Name = "ExceptionWindow";
	        this.ShowIcon = false;
	        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
	        this.Text = "Error!";
	        ((System.ComponentModel.ISupportInitialize)(this.systemBitmap)).EndInit();
	        this.ResumeLayout(false);
	        this.PerformLayout();
        }

        private CBRE.UI.ReadOnlyTextBox ProcessorName;

        private System.Windows.Forms.Button reportButton;

        private CBRE.UI.ReadOnlyTextBox RuntimeVersion;
        private System.Windows.Forms.Label label2;

        private CBRE.UI.ReadOnlyTextBox AvailableMemory;
        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.PictureBox systemBitmap;

        #endregion
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private CBRE.UI.ReadOnlyTextBox OperatingSystem;
        private System.Windows.Forms.Label label6;
        private CBRE.UI.ReadOnlyTextBox CBREVersion;
        private System.Windows.Forms.Label label7;
        private CBRE.UI.ReadOnlyTextBox FullError;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Label HeaderLabel;
    }
}
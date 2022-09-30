using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CBRE.Editor.Logging
{
    public partial class ExceptionWindow : Form
    {
        public ExceptionInfo ExceptionInfo { get; set; }

        public ExceptionWindow(ExceptionInfo info)
        {
            ExceptionInfo = info;
            InitializeComponent();
            FrameworkVersion.Text = info.RuntimeVersion;
            OperatingSystem.Text = info.OperatingSystem;
            CBREVersion.Text = info.ApplicationVersion;
            FullError.Text = info.FullStackTrace;

            FullError.ForeColor = SystemColors.WindowText;
            FullError.BackColor = SystemColors.Control;

            FrameworkVersion.ForeColor = SystemColors.WindowText;
            FrameworkVersion.BackColor = SystemColors.Control;

            CBREVersion.ForeColor = SystemColors.WindowText;
            CBREVersion.BackColor = SystemColors.Control;

            OperatingSystem.ForeColor = SystemColors.WindowText;
            OperatingSystem.BackColor = SystemColors.Control;

            try
            {
                Directory.CreateDirectory("Error Logs");
                string fn = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss");
                using (StreamWriter sw = new StreamWriter($"Error Logs\\{fn}.txt"))
                {
                    string content = "CBRE-EX has encountered an error. Details are found below.\n" +
                                     "-----------------------------------------------------------\n" +
                                     $".NET Version: {info.RuntimeVersion}\n" +
                                     $"Operating System: {info.OperatingSystem}\n" +
                                     $"CBRE-EX Version: {info.ApplicationVersion}\n" +
                                     "-----------------------ERROR MESSAGE-----------------------\n" +
                                     info.FullStackTrace;
                    sw.Write(content);
                }
                HeaderLabel.Text = $"Details have been written to \"Error Logs/{fn}.txt\"";
            }
            catch (Exception e)
            {
                HeaderLabel.Text = $"Couldn't write error log: {e.Message}";
            }

            FullError.SelectionLength = 0;
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(FullError.Text);
        }
    }
}

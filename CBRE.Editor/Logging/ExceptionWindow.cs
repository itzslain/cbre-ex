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

            FullError.ForeColor = Color.Black;
            FullError.BackColor = Color.White;

            FrameworkVersion.ForeColor = Color.Black;
            FrameworkVersion.BackColor = Color.White;

            CBREVersion.ForeColor = Color.Black;
            CBREVersion.BackColor = Color.White;

            OperatingSystem.ForeColor = Color.Black;
            OperatingSystem.BackColor = Color.White;

            //TRY TO WRITE A LOG FILE, IF IT FAILS, DONT DO ANYTHING
            try
            {
                string fn = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss");
                using (StreamWriter sw = new StreamWriter($"Error Logs\\{fn}.txt"))
                {
                    string content = "CBRE found an error and has written a log below.\n" +
                                     "-------------------------------------------------\n" +
                                     $".NET Version: {info.RuntimeVersion}\n" +
                                     $"Operating System: {info.OperatingSystem}\n" +
                                     $"CBRE Version: {info.ApplicationVersion}\n" +
                                     "------------------ERROR MESSAGE------------------\n" +
                                     info.FullStackTrace;
                    sw.Write(content);
                }
                label2.Text = "A log file has been written in the Error Logs folder.";
            }
            catch (Exception)
            {
                label2.Text = "Couldn't write a log file to the Error Logs folder.";
            }
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

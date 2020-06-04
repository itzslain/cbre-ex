using System;
using System.Drawing;
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

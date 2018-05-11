using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Sledge.Editor.Logging
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
            SledgeVersion.Text = info.ApplicationVersion;
            FullError.Text = info.FullStackTrace;
        }
        
        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void ExceptionWindow_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

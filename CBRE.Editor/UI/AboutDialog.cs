using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CBRE.Common.Mediator;

namespace CBRE.Editor.UI
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();

            VersionLabel.Text = FileVersionInfo.GetVersionInfo(typeof (Editor).Assembly.Location).FileVersion;

            LTLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://logic-and-trick.com");
            GithubLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/juanjp600/sledge");
            GPLLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://www.gnu.org/licenses/gpl-2.0.html");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void GithubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}

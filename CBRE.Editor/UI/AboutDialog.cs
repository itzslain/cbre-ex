using CBRE.Common.Mediator;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();

            VersionLabel.Text = "Version " + Assembly.GetAssembly(typeof(Editor)).GetName().Version.ToString(2);

            GitHubLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/AestheticalZ/cbre-ex");
            OriginalLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/SCP-CBN/cbre");
            GPLLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://www.gnu.org/licenses/gpl-2.0.html");

			DescriptionLabel.Links.Add(221, 19, "http://logic-and-trick.com");
			DescriptionLabel.LinkClicked += DescriptionLabel_LinkClicked;
        }

		private void DescriptionLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(e.Link.LinkData.ToString());
		}
	}
}

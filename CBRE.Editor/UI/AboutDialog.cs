using System.Diagnostics;
using System.Windows.Forms;
using CBRE.Common.Mediator;

namespace CBRE.Editor.UI {
    public partial class AboutDialog : Form {
        public AboutDialog() {
            InitializeComponent();

            VersionLabel.Text = "v" + FileVersionInfo.GetVersionInfo(typeof(Editor).Assembly.Location).FileVersion;

            LTLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://logic-and-trick.com");
            GithubLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/AestheticalZ/cbre-ex");
            OriginalLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/SCP-CBN/cbre");
            GPLLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://www.gnu.org/licenses/gpl-2.0.html");
        }
    }
}

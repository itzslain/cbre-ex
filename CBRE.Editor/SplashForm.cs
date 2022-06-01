using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CBRE.Editor
{
	public partial class SplashForm : Form
	{
		public SplashForm()
		{
			InitializeComponent();

			this.AllowTransparency = true;
			this.TransparencyKey = Color.Fuchsia;

			BottomPanelLabel.Text = BottomPanelLabel.Text.Replace("(ver)", Assembly.GetAssembly(typeof(Editor)).GetName().Version.ToString(2));
		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{
			Control target = sender as Control;

			ControlPaint.DrawBorder3D(e.Graphics, target.ClientRectangle, Border3DStyle.Raised);
		}
	}
}

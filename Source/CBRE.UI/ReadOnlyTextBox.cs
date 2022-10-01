using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CBRE.UI
{
    public class ReadOnlyTextBox : TextBox
    {
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public ReadOnlyTextBox()
        {
            this.ReadOnly = true;
            
            this.GotFocus += OnGotFocus;
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            HideCaret(this.Handle);
        }
    }
}
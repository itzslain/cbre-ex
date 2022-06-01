using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CBRE.QuickForms.Items
{
    /// <summary>
    /// A control that shows a button.
    /// </summary>
    public class QuickFormButton : QuickFormItem
    {
        private readonly Action _clickevent;

        public QuickFormButton(string text, Action click)
        {
            Name = text;
            _clickevent = click;
        }

        public override List<Control> GetControls(QuickForm qf)
        {
            List<Control> controls = new List<Control>();

			Button b = new Button() { Font = SystemFonts.MessageBoxFont, FlatStyle = FlatStyle.System };
            b.Click += (sender, e) => _clickevent();
            b.Width = 120;
            b.Text = Name;
            Location(b, qf, true);
            controls.Add(b);

            return controls;
        }
    }
}

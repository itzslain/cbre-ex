using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// TODO: Fix this cancer
using static CBRE.Editor.UI.UpdaterForm;

namespace CBRE.Editor
{
    public partial class EntityLoadForm : Form
    {
        public EntityLoadForm(List<string> Errors)
        {
            InitializeComponent();

            SHSTOCKICONINFO StockIconInfo = new SHSTOCKICONINFO();
            StockIconInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO));
            SHGetStockIconInfo(SHSTOCKICONID.SIID_WARNING, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SHELLICONSIZE, ref StockIconInfo);

            systemBitmap.Image = Icon.FromHandle(StockIconInfo.hIcon).ToBitmap();

            string joinedText = string.Join(System.Environment.NewLine, Errors);

            this.errorTextBox.Text += joinedText;

            try
            {
                Directory.CreateDirectory("Logs\\Entities");
                string filename = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss") + ".txt";

                using (StreamWriter streamWriter = new StreamWriter($"Logs\\Entities\\{filename}"))
                {
                    string content = "CBRE-EX has encountered errors when loading custom entities. Details can be found below.\n" +
                                     "----------------------------------------------------------------------------------------\n" +
                                     joinedText;
                    streamWriter.Write(content);
                }

                logLabel.Text += $"Details have been written to \"Logs\\Entities\\{filename}\"";
            }
            catch (Exception ex)
            {
                logLabel.Text += $"Could not write error log: {ex.Message}";
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(this.errorTextBox.Text);
        }
    }
}

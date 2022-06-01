using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    public partial class UpdaterForm : Form
    {
        private string DownloadUri;
        private string ChecksumUri;
        private string VersionString;

        public UpdaterForm(Version version, string changelog, string url, string checksumUrl)
        {
            InitializeComponent();
            DownloadUri = url;
            ChecksumUri = checksumUrl;
            VersionString = version.ToString(2);

            SHSTOCKICONINFO stockIconInfo = new SHSTOCKICONINFO();
            stockIconInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO));
            SHGetStockIconInfo(SHSTOCKICONID.SIID_INFO, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SHELLICONSIZE, ref stockIconInfo);

            systemBitmap.Image = Icon.FromHandle(stockIconInfo.hIcon).ToBitmap();

            headerLabel.Text = headerLabel.Text.Replace("(version)", VersionString);

            changelogBox.Text = changelog;
            changelogBox.BackColor = SystemColors.Window;
            changelogBox.GotFocus += ChangelogGotFocus;
        }

        private void ChangelogGotFocus(object sender, EventArgs e)
        {
            HideCaret(changelogBox.Handle);
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void yesButton_Click(object sender, EventArgs e)
        {
            string CurrentFilename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            this.changelogBox.Select(0, 0);
            this.ControlBox = false;
            this.noButton.Enabled = false;
            this.yesButton.Enabled = false;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string downloadedChecksum = string.Empty;

                    webClient.Headers.Add("User-Agent", "AestheticalZ/cbre-ex");

                    webClient.DownloadFile(new Uri(ChecksumUri), "CHECKSUM.txt");
                    downloadedChecksum = File.ReadAllText("CHECKSUM.txt");
                    if (string.IsNullOrEmpty(downloadedChecksum)) throw new Exception("The checksum file was empty.");

                    webClient.DownloadProgressChanged += (senderObj, eventArg) =>
                    {
                        downloadProgress.Value = eventArg.ProgressPercentage;
                        statusLabel.Text = $"Status: Downloading {eventArg.ProgressPercentage}%";
                    };

                    await webClient.DownloadFileTaskAsync(new Uri(DownloadUri), "Update.zip");

                    statusLabel.Text = "Status: Verifying...";

                    using (MD5 md5 = MD5.Create())
                    {
                        using (FileStream stream = File.OpenRead("Update.zip"))
                        {
                            string convertedChecksum;

                            byte[] hash = md5.ComputeHash(stream);
                            convertedChecksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                            if (downloadedChecksum != convertedChecksum) throw new Exception("Verification failed. Update package is probably corrupted.");
                        }
                    }

                    ProcessStartInfo updaterProcess = new ProcessStartInfo("CBRE.Updater.exe");
                    updaterProcess.Arguments = $"{VersionString} {FixSpaces(CurrentFilename)}";
                    updaterProcess.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    updaterProcess.UseShellExecute = true;
                    Process.Start(updaterProcess);

                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                this.ControlBox = true;
                this.noButton.Enabled = true;
                this.yesButton.Enabled = true;
                this.downloadProgress.Value = 0;
                statusLabel.Text = "Status: Idle";

                MessageBox.Show("An error has ocurred while downloading and verifying the update package.\n" +
                               $"{ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FixSpaces(string text)
        {
            if (text.Contains(" ")) return $"\"{text}\"";
            else return text;
        }
    }
}

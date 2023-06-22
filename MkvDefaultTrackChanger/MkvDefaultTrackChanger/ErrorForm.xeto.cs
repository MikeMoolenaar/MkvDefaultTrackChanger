using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace MkvDefaultTrackChanger
{
    public class ErrorForm : Form
    {
        TextArea txaExceptionMessage;
        private Label lblTitle;
        public ErrorForm(Exception ex, string? mkvFileInfo)
        {
            XamlReader.Load(this);
            txaExceptionMessage.Text = new StringBuilder()
                .Append(GetPlatformInfo())
                .AppendLine()
                .AppendLine(mkvFileInfo ?? "No mkv file info.")
                .AppendLine()
                .AppendLine(ex.ToString())
                .ToString();
            lblTitle.Font = new Font(lblTitle.Font.Family, 20);
        }

        private string GetPlatformInfo()
        {
            return new StringBuilder()
                .AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}")
                .AppendLine($"RuntimeIdentifier: {RuntimeInformation.RuntimeIdentifier}")
                .AppendLine($"OS: {RuntimeInformation.OSDescription}")
                .ToString();
        }

        protected void BtnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
        
        protected void BtnCreateIssueClicked(object sender, EventArgs e)
        {
            Application.Instance.Open("https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/issues/new");
        }
    }
}
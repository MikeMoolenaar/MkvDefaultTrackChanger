using Eto.Forms;
using Eto.Serialization.Xaml;
using System;
using MatroskaTest;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// TODO add support for changing multiple files and 
// TODO make styling pretty, also add a progressbar
namespace EtoTest
{
    public class MainForm : Form
    {
        FilePicker filePicker;
        ListBox listbox;
        DropDown dropdownAudio;
        DropDown dropdownSubtitles;
        CheckBox chCopyBeforeChange;
        Button btnApply;
        MkvFilesContainer mkvContainer;
        private bool needsReload = false;

        public MainForm()
        {
            XamlReader.Load(this);
            filePicker.Filters.Add(new FileFilter("MKV files", "*.mkv"));
        }

        private void FilePickerSelectionDone(object sender, EventArgs e)
        {
            this.LoadFiles();
        }

        private void LoadFiles()
        {
            if (string.IsNullOrWhiteSpace(filePicker.FilePath)) return;
            this.listbox.Items.Clear();

            string[] filePaths = filePicker.FilePath.Split('|');
            foreach (var filename in filePaths)
            {
                this.listbox.Items.Add(filename);
            }

            this.mkvContainer = new MkvFilesContainer(filePaths);
            if (this.mkvContainer.lsMkFilesRejected.Count > 0)
            {
                string rejectedFiles = Environment.NewLine + Environment.NewLine;
                this.mkvContainer.lsMkFilesRejected.ForEach((x) =>
                {
                    rejectedFiles += Path.GetFileName(x.filePath) + Environment.NewLine + Environment.NewLine;
                });
                MessageBox.Show("The following files were rejected: " + rejectedFiles, MessageBoxType.Warning);
            }

            var lsSubtitleTracks = this.mkvContainer.GetSubtitleTracks();
            var lsAudioTracks = this.mkvContainer.GetAudioTracks();

            // TODO clean up the selectedkey parts, put them in a method!!
            this.dropdownSubtitles.Items.Clear();
            this.dropdownSubtitles.Items.AddRange(lsSubtitleTracks.ToEnoListItems());
            this.dropdownSubtitles.SelectedKey = lsSubtitleTracks
                .FirstOrDefault(x => x.flagDefault || x.flagForced)
                ?.number.ToString();
            this.dropdownAudio.Items.Clear();
            this.dropdownAudio.Items.AddRange(lsAudioTracks.ToEnoListItems());
            this.dropdownAudio.SelectedKey = lsAudioTracks
                .FirstOrDefault(x => x.flagDefault || x.flagForced)
                ?.number.ToString();
            if (this.dropdownAudio.SelectedKey is null)
                this.dropdownAudio.SelectedKey = lsAudioTracks[0].number.ToString();
            if (this.dropdownSubtitles.SelectedKey is null)
                this.dropdownSubtitles.SelectedKey = lsSubtitleTracks[0].number.ToString();
        }

        protected void BtnApplyClicked(object sender, EventArgs e)
        {
            if (this.chCopyBeforeChange.Checked == true)
            {
                var filePath = mkvContainer.lsMkvFiles[0].filePath;
                string dir = Path.GetDirectoryName(filePath);
                File.Copy(filePath, $"{dir}\\Copy.mkv", true);
            }
            
            this.btnApply.Enabled = false;
            this.mkvContainer.WriteChanges((Track t) =>
            {
                t.flagDefault = this.isSelectedTrack(t);
            });
            this.btnApply.Enabled = true;
            this.LoadFiles();
        }

        private bool isSelectedTrack(Track t)
        {
            string key = t.number.ToString();
            return this.dropdownAudio.SelectedKey == key || this.dropdownSubtitles.SelectedKey == key;
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog(this);
        }

        protected void HandleQuit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }
    }
}

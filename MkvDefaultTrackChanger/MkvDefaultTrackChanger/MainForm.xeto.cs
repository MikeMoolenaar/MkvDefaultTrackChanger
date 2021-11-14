using Eto.Forms;
using Eto.Serialization.Xaml;
using System;
using System.Collections.Generic;
using MatroskaLib;
using System.Linq;
using System.IO;

namespace MkvDefaultTrackChanger
{
    public class MainForm : Form
    {
        FilePicker filePicker;
        Label lblFilesSelected;
        DropDown dropdownAudio;
        DropDown dropdownSubtitles;
        Button btnApply;
        Label lblStatus;
        MkvFilesContainer mkvContainer;

        public MainForm()
        {
            XamlReader.Load(this);
            filePicker.Filters.Add(new FileFilter("MKV files", "*.mkv"));
        }

        private void FilePickerSelectionDone(object sender, EventArgs e)
        {
            try
            {
                this.LoadFiles();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }
        
        private void LoadFiles()
        {
            if (string.IsNullOrWhiteSpace(filePicker.FilePath)) return;

            string[] filePaths = filePicker.FilePath.Split('|');
            this.lblFilesSelected.Text = $"{filePaths.Length} files selected";

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

            this.FillDropdown(this.dropdownSubtitles, lsSubtitleTracks);
            this.FillDropdown(this.dropdownAudio, lsAudioTracks);

            this.btnApply.Enabled = true;
            this.lblStatus.Text = "";
        }

        private void FillDropdown(DropDown dropDown, List<Track> lsTracks)
        {
            dropDown.Items.Clear();
            dropDown.Items.AddRange(lsTracks.ToEnoListItems());
            dropDown.SelectedKey = lsTracks
                .FirstOrDefault(x => x.flagDefault || x.flagForced)
                ?.number.ToString();
            if (dropDown.SelectedKey is null)
                dropDown.SelectedKey = lsTracks[0].number.ToString();
        }

        protected void BtnApplyClicked(object sender, EventArgs e)
        {
            try
            {
                this.btnApply.Enabled = false;
                this.mkvContainer.WriteChanges((Track t) => { t.flagDefault = this.IsSelectedTrack(t); });
                this.btnApply.Enabled = true;
                this.LoadFiles();
                this.lblStatus.Text = "Done!";
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        private bool IsSelectedTrack(Track t)
        {
            string key = t.number.ToString();
            return this.dropdownAudio.SelectedKey == key || this.dropdownSubtitles.SelectedKey == key;
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            var aboutDialog = new AboutDialog()
            {
                // TODO logo
                // Logo = 
                Website = new Uri("https://github.com/MikeMoolenaar/MkvDefaultTrackChanger"),
                WebsiteLabel = "Github page",
                ProgramDescription =
                    @"MkvDefaultTrackChanger is a small application to change the default subtitle and audio tracks in MKV video files. ",
                License = @"Copyright (C) 2021 Mike Moolenaar
MkvDefaultTrackChanger is licensed under the terms of the GNU General Public License version 3. A copy of this license can be obtained from <https://www.gnu.org/licenses/gpl-3.0.html>.",
                Developers = new[] {"Mike Moolenaar"}
            };
            aboutDialog.ShowDialog(this);
        }

        private void HandleException(Exception ex)
        {
            new ErrorForm(ex, this.mkvContainer?.ToString()).Show();
        }
    }
}
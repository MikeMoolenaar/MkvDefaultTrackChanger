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
        Label lblFilesSelected;
        DropDown dropdownAudio;
        DropDown dropdownSubtitles;
        Button btnApply;
        Label lblStatus;
        
        MkvFilesContainer mkvContainer;
        OpenFileDialog fileDialog;

        public MainForm()
        {
            XamlReader.Load(this);
            
            fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
            fileDialog.MultiSelect = true;
        }

        private void BtnBrowseFilesClick(object sender, EventArgs e)
        {
            var dialogResult = fileDialog.ShowDialog(this);
            
            if (dialogResult == DialogResult.Ok)
            {
                try
                {
                    LoadFiles();
                }
                catch (Exception exception)
                {
                    HandleException(exception);
                }
            }
        }
        
        private void LoadFiles()
        {
            string[] filePaths = fileDialog.Filenames.ToArray();

            string files = filePaths.Length == 1 ? "file" : "files";
            lblFilesSelected.Text = $"{filePaths.Length} {files} selected";

            mkvContainer = new MkvFilesContainer(filePaths);
            if (mkvContainer.lsMkFilesRejected.Count > 0)
            {
                string rejectedFiles = Environment.NewLine + Environment.NewLine;
                mkvContainer.lsMkFilesRejected.ForEach((x) =>
                {
                    rejectedFiles += Path.GetFileName(x.filePath) + Environment.NewLine + Environment.NewLine;
                });
                MessageBox.Show("The following files were rejected: " + rejectedFiles, MessageBoxType.Warning);
            }

            var lsSubtitleTracks = mkvContainer.GetSubtitleTracks();
            var lsAudioTracks = mkvContainer.GetAudioTracks();

            FillDropdown(dropdownSubtitles, lsSubtitleTracks);
            FillDropdown(dropdownAudio, lsAudioTracks);

            btnApply.Enabled = true;
            lblStatus.Text = "";
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
                btnApply.Enabled = false;
                mkvContainer.WriteChanges((Track t) => { t.flagDefault = IsSelectedTrack(t); });
                btnApply.Enabled = true;
                LoadFiles();
                lblStatus.Text = "Done!";
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        private bool IsSelectedTrack(Track t)
        {
            string key = t.number.ToString();
            return dropdownAudio.SelectedKey == key || dropdownSubtitles.SelectedKey == key;
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            var aboutDialog = new AboutDialog
            {
                // TODO logo
                // Logo = 
                Website = new Uri("https://github.com/MikeMoolenaar/MkvDefaultTrackChanger"),
                WebsiteLabel = "Github",
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
            new ErrorForm(ex, mkvContainer?.ToString()).Show();
        }
    }
}
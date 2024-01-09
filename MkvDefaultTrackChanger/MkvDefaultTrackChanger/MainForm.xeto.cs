using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MatroskaLib;
using MatroskaLib.Types;

namespace MkvDefaultTrackChanger;

public class MainForm : Form
{
    Label lblFilesSelected;
    DropDown dropdownAudio;
    DropDown dropdownSubtitles;
    Button btnApply;
    Label lblStatus;

    MkvFilesContainer mkvContainer;
    OpenFileDialog fileDialog;
    private (string audio, string subtitles)? appliedConfig;

    public MainForm()
    {
        Icon = Icon.FromResource("MkvDefaultTrackChanger.logo.ico");
        XamlReader.Load(this);

        fileDialog = new OpenFileDialog();
        fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
        fileDialog.MultiSelect = true;
    }

    private void BtnBrowseFilesClick(object sender, EventArgs e)
    {
        var dialogResult = fileDialog.ShowDialog(this);
        if (dialogResult != DialogResult.Ok) return;

        try
        {
            LoadFiles();
            
            btnApply.Enabled = true;
            lblStatus.Text = string.Empty;
            appliedConfig = null;
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }

    private void LoadFiles()
    {
        string[] filePaths = fileDialog.Filenames.ToArray();

        mkvContainer = new MkvFilesContainer(filePaths);
        if (mkvContainer.MkFilesRejected.Count > 0)
        {
            string rejectedFiles = Environment.NewLine + Environment.NewLine;
            mkvContainer.MkFilesRejected.ForEach((x) =>
            {
                rejectedFiles += Path.GetFileName(x.filePath) + Environment.NewLine + Environment.NewLine;
            });
            MessageBox.Show("The following files were rejected: " + rejectedFiles, MessageBoxType.Warning);
        }

        var lsSubtitleTracks = mkvContainer.GetSubtitleTracks();
        var lsAudioTracks = mkvContainer.GetAudioTracks();

        FillDropdown(dropdownSubtitles, lsSubtitleTracks);
        FillDropdown(dropdownAudio, lsAudioTracks);
        
        string files = filePaths.Length == 1 ? "file" : "files";
        lblFilesSelected.Text = $"{filePaths.Length} {files} selected";
    }

    private void FillDropdown(DropDown dropDown, List<Track> lsTracks)
    {
        dropDown.Items.Clear();
        dropDown.Items.AddRange(lsTracks.ToEnoListItems());
        dropDown.SelectedKey = lsTracks
            .FirstOrDefault(x => x.flagDefault || x.flagForced)
            ?.number.ToString();
        dropDown.Enabled = true;
        if (dropDown.SelectedKey is null)
            dropDown.SelectedKey = lsTracks[0].number.ToString();
    }

    protected void BtnApplyClicked(object sender, EventArgs e)
    {
        try
        {
            btnApply.Enabled = false;
            mkvContainer.WriteChanges(track =>
            {
                string key = track.number.ToString();
                track.flagDefault = dropdownAudio.SelectedKey == key || dropdownSubtitles.SelectedKey == key;
            });
            
            LoadFiles();
            
            appliedConfig = (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey);
            lblStatus.Text = "Done!";
        }
        catch (Exception exception)
        {
            HandleException(exception);
            btnApply.Enabled = true;
        }
    }

    private void OnDropdownSelectionChanged(object? sender, EventArgs e)
    {
        if (appliedConfig != (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey))
        {
            btnApply.Enabled = true;
            lblStatus.Text = string.Empty;
        }
        else
        {
            btnApply.Enabled = false;
            lblStatus.Text = "Done!";
        }
       
    }

    protected void HandleAbout(object sender, EventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            Logo = Icon,
            Website = new Uri("https://github.com/MikeMoolenaar/MkvDefaultTrackChanger"),
            WebsiteLabel = "Github",
            ProgramDescription =
                "MkvDefaultTrackChanger is a small application to change the default subtitle and audio tracks in MKV video files. ",
            License = @"Copyright (C) 2021 Mike Moolenaar
MkvDefaultTrackChanger is licensed under the terms of the GNU General Public License version 3. A copy of this license can be obtained from <https://www.gnu.org/licenses/gpl-3.0.html>.",
            Developers = ["Mike Moolenaar"]
        };
        aboutDialog.ShowDialog(this);
    }

    private void HandleException(Exception ex)
    {
        new ErrorForm(ex, mkvContainer?.ToString(), Icon).Show();
    }
}

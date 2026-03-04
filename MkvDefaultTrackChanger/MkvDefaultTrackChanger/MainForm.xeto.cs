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
    Button btnPrevious;
    Button btnNext;
    Label lblCurrentFile;
    Label lblCurrentAudio;
    Label lblCurrentSubtitles;

    List<MkvFile> mkvFiles;
    int currentFileIndex;
    OpenFileDialog fileDialog;
    private Dictionary<string, (string audio, string subtitles)> appliedConfigs;

    public MainForm()
    {
        Icon = Icon.FromResource("MkvDefaultTrackChanger.logo.ico");
        XamlReader.Load(this);

        fileDialog = new OpenFileDialog();
        fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
        fileDialog.MultiSelect = true;
        appliedConfigs = new Dictionary<string, (string audio, string subtitles)>();
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
            appliedConfigs.Clear();
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }

    private void LoadFiles()
    {
        string[] filePaths = fileDialog.Filenames.ToArray();

        mkvFiles = MatroskaReader.ReadMkvFiles(filePaths);
        currentFileIndex = 0;
        
        string files = filePaths.Length == 1 ? "file" : "files";
        lblFilesSelected.Text = $"{filePaths.Length} {files} selected";
        
        LoadCurrentFile();
        UpdateNavigationButtons();
    }

    private void LoadCurrentFile()
    {
        if (mkvFiles == null || mkvFiles.Count == 0) return;

        var currentFile = mkvFiles[currentFileIndex];
        
        MatroskaReader.LoadMediaInfoForFile(currentFile);
        
        var lsSubtitleTracks = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.subtitle)
            .ToList();
        var lsAudioTracks = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.audio)
            .ToList();

        lsSubtitleTracks.Insert(0, new TrackDisable());

        FillDropdown(dropdownSubtitles, lsSubtitleTracks);
        FillDropdown(dropdownAudio, lsAudioTracks);
        
        UpdateCurrentTrackLabels(lsAudioTracks, lsSubtitleTracks);
        
        lblCurrentFile.Text = $"File {currentFileIndex + 1} of {mkvFiles.Count}: {Path.GetFileName(currentFile.filePath)}";

        if (appliedConfigs.TryGetValue(currentFile.filePath, out var config))
        {
            dropdownAudio.SelectedKey = config.audio;
            dropdownSubtitles.SelectedKey = config.subtitles;
        }
    }

    private void UpdateCurrentTrackLabels(List<Track> audioTracks, List<Track> subtitleTracks)
    {
        var defaultAudio = audioTracks.FirstOrDefault(x => x.flagDefault);
        lblCurrentAudio.Text = defaultAudio != null 
            ? $"Current default: {defaultAudio.ToUiString()}"
            : "Current default: None";
        
        var defaultSubtitle = subtitleTracks.FirstOrDefault(x => x.flagDefault);
        lblCurrentSubtitles.Text = defaultSubtitle != null 
            ? $"Current default: {defaultSubtitle.ToUiString()}"
            : "Current default: None";
    }

    private void UpdateNavigationButtons()
    {
        btnPrevious.Enabled = currentFileIndex > 0;
        btnNext.Enabled = currentFileIndex < mkvFiles.Count - 1;
    }

    private void FillDropdown(DropDown dropDown, List<Track> lsTracks)
    {
        dropDown.Items.Clear();
        dropDown.Items.AddRange(lsTracks.ToEnoListItems());
        dropDown.SelectedKey = lsTracks
            .FirstOrDefault(x => x.flagDefault || x.flagForced)
            ?.number.ToString();
        dropDown.Enabled = true;
        if (dropDown.SelectedKey is null && lsTracks.Count > 0)
            dropDown.SelectedKey = lsTracks[0].number.ToString();
    }

    protected void BtnApplyClicked(object sender, EventArgs e)
    {
        try
        {
            btnApply.Enabled = false;
            
            var currentFile = mkvFiles[currentFileIndex];
            
            currentFile.tracks.ForEach(track =>
            {
                string key = track.number.ToString();
                track.flagDefault = dropdownAudio.SelectedKey == key || dropdownSubtitles.SelectedKey == key;
            });
            
            MatroskaWriter.WriteMkvFile(currentFile);
            
            appliedConfigs[currentFile.filePath] = (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey);
            
            mkvFiles[currentFileIndex] = MatroskaReader.ReadMkvFiles([currentFile.filePath])[0];
            LoadCurrentFile();
            
            int completed = appliedConfigs.Count;
            int total = mkvFiles.Count;
            lblStatus.Text = $"Saved! ({completed}/{total} files processed)";
        }
        catch (Exception exception)
        {
            HandleException(exception);
            btnApply.Enabled = true;
        }
    }

    protected void BtnPreviousClicked(object sender, EventArgs e)
    {
        if (currentFileIndex > 0)
        {
            currentFileIndex--;
            LoadCurrentFile();
            UpdateNavigationButtons();
            UpdateApplyButtonState();
        }
    }

    protected void BtnNextClicked(object sender, EventArgs e)
    {
        if (currentFileIndex < mkvFiles.Count - 1)
        {
            currentFileIndex++;
            LoadCurrentFile();
            UpdateNavigationButtons();
            UpdateApplyButtonState();
        }
    }

    private void OnDropdownSelectionChanged(object? sender, EventArgs e)
    {
        UpdateApplyButtonState();
    }

    private void UpdateApplyButtonState()
    {
        if (mkvFiles == null || mkvFiles.Count == 0) return;

        var currentFile = mkvFiles[currentFileIndex];
        if (appliedConfigs.TryGetValue(currentFile.filePath, out var config) &&
            config == (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey))
        {
            btnApply.Enabled = false;
            int completed = appliedConfigs.Count;
            int total = mkvFiles.Count;
            lblStatus.Text = $"Saved! ({completed}/{total} files processed)";
        }
        else
        {
            btnApply.Enabled = true;
            lblStatus.Text = string.Empty;
        }
    }

    protected void HandleAbout(object sender, EventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            Logo = Icon.WithSize(100, 200),
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
        var filePath = mkvFiles?[currentFileIndex]?.filePath;
        new ErrorForm(ex, filePath ?? "Unknown file", Icon).Show();
    }
}

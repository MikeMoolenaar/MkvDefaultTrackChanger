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

public sealed class MainForm : Form
{
    Label lblFilesSelected;
    DropDown dropdownAudio;
    DropDown dropdownSubtitles;
    Button btnApply;
    Label lblStatus;
    Label lblDragDrop;

    MkvFilesContainer mkvContainer;
    OpenFileDialog fileDialog;
    private List<string> currentFilePaths = new();
    private List<MkvFileGroup> groups = new();
    private int currentGroupIndex;
    private List<Track> currentAudioTracks = new();
    private List<Track> currentSubtitleTracks = new();
    private (string audio, string subtitles)? appliedConfig;

    public MainForm()
    {
        Icon = Icon.FromResource("MkvDefaultTrackChanger.logo.ico");
        XamlReader.Load(this);

        fileDialog = new OpenFileDialog();
        fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
        fileDialog.MultiSelect = true;

        AllowDrop = !Platform.IsGtk; // Can't seem to get this to work in Wayland...

        lblDragDrop!.Visible = AllowDrop;
    }

    private void BtnBrowseFilesClick(object sender, EventArgs e)
    {
        var dialogResult = fileDialog.ShowDialog(this);
        if (dialogResult != DialogResult.Ok) return;

        ProcessFiles(fileDialog.Filenames.ToList());
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = GetDragEventFilePaths(e).Count > 0 ? DragEffects.Copy : DragEffects.None;
    }

    private void OnDragDrop(object sender, DragEventArgs e)
    {
        var filePaths = GetDragEventFilePaths(e);
        if (filePaths.Count > 0)
            ProcessFiles(filePaths);
    }

    private List<string> GetDragEventFilePaths(DragEventArgs e)
    {
        try
        {
            if (!e.Data.ContainsUris)
                return [];

            return e.Data.Uris
                .Where(uri => uri.IsFile)
                .Select(uri => uri.LocalPath)
                .Where(path => path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return [];
        }
    }

    private void ProcessFiles(List<string> filePaths)
    {
        try
        {
            currentFilePaths = filePaths;
            currentGroupIndex = 0;
            RebuildGroups();
            ShowGroup(preferredAudio: null, preferredSubtitle: null);
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }

    private void RebuildGroups()
    {
        mkvContainer = new MkvFilesContainer(currentFilePaths);
        groups = mkvContainer.Groups;
        if (currentGroupIndex >= groups.Count)
            currentGroupIndex = groups.Count - 1;
    }

    private void ShowGroup(Track? preferredAudio, Track? preferredSubtitle)
    {
        var group = groups[currentGroupIndex];

        currentAudioTracks = mkvContainer.GetAudioTracks(group);
        currentSubtitleTracks = mkvContainer.GetSubtitleTracks(group);

        FillDropdown(dropdownAudio, currentAudioTracks, preferredAudio);
        FillDropdown(dropdownSubtitles, currentSubtitleTracks, preferredSubtitle);

        var fileNames = group.Files.Select(f => Path.GetFileName(f.filePath)).ToList();
        string filesWord = fileNames.Count == 1 ? "file" : "files";
        string groupPrefix = groups.Count > 1 ? $"Group {currentGroupIndex + 1}/{groups.Count} — " : "";

        const int maxNamesInline = 3;
        string namesInline = fileNames.Count <= maxNamesInline
            ? string.Join(", ", fileNames)
            : string.Join(", ", fileNames.Take(maxNamesInline)) + $", +{fileNames.Count - maxNamesInline} more";

        lblFilesSelected.Text = $"{groupPrefix}{fileNames.Count} {filesWord}: {namesInline}";
        lblFilesSelected.ToolTip = string.Join(Environment.NewLine, fileNames);
        lblDragDrop.Visible = false;

        btnApply.Enabled = true;
        lblStatus.Text = string.Empty;
        appliedConfig = null;
    }

    private void FillDropdown(DropDown dropDown, List<Track> lsTracks, Track? preferred)
    {
        dropDown.Items.Clear();
        dropDown.Items.AddRange(lsTracks.ToEnoListItems());

        string? selectedKey = preferred switch
        {
            TrackDisable => lsTracks.OfType<TrackDisable>().FirstOrDefault()?.number.ToString(),
            not null => lsTracks.FirstOrDefault(x => x is not TrackDisable && x.language == preferred.language)?.number.ToString(),
            null => null
        };

        selectedKey ??= lsTracks
            .FirstOrDefault(x => x.flagDefault || x.flagForced)
            ?.number.ToString();

        dropDown.SelectedKey = selectedKey ?? lsTracks[0].number.ToString();
        dropDown.Enabled = true;
    }

    private void BtnApplyClicked(object sender, EventArgs e)
    {
        try
        {
            btnApply.Enabled = false;
            var group = groups[currentGroupIndex];
            string selectedAudioKey = dropdownAudio.SelectedKey;
            string selectedSubtitleKey = dropdownSubtitles.SelectedKey;

            Track? preferredAudio = currentAudioTracks.FirstOrDefault(t => t.number.ToString() == selectedAudioKey);
            Track? preferredSubtitle = currentSubtitleTracks.FirstOrDefault(t => t.number.ToString() == selectedSubtitleKey);

            mkvContainer.WriteChanges(group, track =>
            {
                string key = track.number.ToString();
                track.flagDefault = selectedAudioKey == key || selectedSubtitleKey == key;
            });

            bool isLastGroup = currentGroupIndex >= groups.Count - 1;
            RebuildGroups();
            if (!isLastGroup)
                currentGroupIndex++;

            ShowGroup(preferredAudio, preferredSubtitle);

            if (isLastGroup)
            {
                appliedConfig = (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey);
                lblStatus.Text = "Done!";
            }
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

    private void HandleAbout(object sender, EventArgs e)
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
            Developers = ["Mike Moolenaar"],
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()?[..^2] ?? "Unknown"
        };
        aboutDialog.ShowDialog(this);
    }

    private void HandleException(Exception ex)
    {
        if (ex is IOException { Message: { } message } && message.Contains("because it is being used by another process", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show($"One of the mkv files is currently in use by another process.{Environment.NewLine}Please close any applications that may be using the file and try again, for example video applications like VLC.",
                MessageBoxType.Error);
            return;
        }

        if (ex is UnauthorizedAccessException)
        {
            var additionalText = string.Empty;
            if (Platform.IsWinForms)
                additionalText += $"{Environment.NewLine}{Environment.NewLine}Double check if the file is not set to Read-only via the properties.";

            MessageBox.Show($"You do not have permission to access one of the mkv files.{Environment.NewLine}Please check the file permissions and try again.{additionalText}",
                MessageBoxType.Error);
            return;
        }

        new ErrorForm(ex, mkvContainer?.ToString(), Icon).Show();
    }
}

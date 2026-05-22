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
    Button btnApplyAll;
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

    public MainForm(String[] args)
    {
        int indexaudio;
        int indexsubtitles;
        string[] filepaths;
        int i;

        Icon = Icon.FromResource("MkvDefaultTrackChanger.logo.ico");
        XamlReader.Load(this);

        appliedConfigs = new Dictionary<string, (string audio, string subtitles)>();

        if (args.Length >= 3)
        {
            indexaudio = int.Parse(args[0]) - 1;
            indexsubtitles = int.Parse(args[1]);
            List<string> templist = new List<string>();
            for (i = 2; i < args.Length; i++)
            {
                templist.Add(args[i]);
            }
            filepaths = templist.ToArray();
            RunCommandLine(indexaudio, indexsubtitles, filepaths);
        }
        else if (args.Length > 0)
        {
            MessageBox.Show($"There must be at least three command line arguments if they are used.",
                MessageBoxType.Error);
            return;
        }
        else
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
            fileDialog.MultiSelect = true;
        }

    }

    void RunCommandLine(int indexaudio, int indexsubtitles, string[] filepaths)
    {
        LoadFilesSub(filepaths);
        for (currentFileIndex = 0; currentFileIndex < filepaths.Length; currentFileIndex++)
        {
            LoadCurrentFile();

            if (indexaudio < 0 || indexaudio > dropdownAudio.Items.Count - 1)
            {
                MessageBox.Show($"Invalid audio track index", MessageBoxType.Error);
                return;
            }
            else
            {
                dropdownAudio.SelectedIndex = indexaudio;
            }

            if (indexsubtitles < 0 || indexsubtitles > dropdownSubtitles.Items.Count - 1)
            {
                MessageBox.Show($"Invalid subtitle track index", MessageBoxType.Error);
                return;
            }
            else
            {
                dropdownSubtitles.SelectedIndex = indexsubtitles;
            }

            BtnApplyClickedSub();

        }
        Environment.Exit(0);
    }

    private void BtnBrowseFilesClick(object sender, EventArgs e)
    {
        var dialogResult = fileDialog.ShowDialog(this);
        if (dialogResult != DialogResult.Ok) return;

        try
        {
            LoadFiles();

            btnApply.Enabled = true;
            btnApplyAll.Enabled = true;
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
        LoadFilesSub(filePaths);
    }

    private void LoadFilesSub(string[] filePaths)
    {
        //string[] filePaths = fileDialog.Filenames.ToArray();

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
        BtnApplyClickedSub();
    }

    protected void BtnApplyAllClicked(object sender, EventArgs e)
    {
        int indexaudio = dropdownAudio.SelectedIndex;
        int indexsubtitles = dropdownSubtitles.SelectedIndex;

        btnApply.Enabled = false;
        btnApplyAll.Enabled = false;

        for (currentFileIndex = 0; currentFileIndex < mkvFiles.Count; currentFileIndex++)
        {
            bool processfile = true;
            LoadCurrentFile();
            UpdateNavigationButtons();

            if (indexaudio < 0 || indexaudio > dropdownAudio.Items.Count - 1)
            {
                MessageBox.Show($"Invalid audio track index", MessageBoxType.Error);
                processfile = false;
            }
            else
            {
                dropdownAudio.SelectedIndex = indexaudio;
            }

            if (indexsubtitles < 0 || indexsubtitles > dropdownSubtitles.Items.Count - 1)
            {
                MessageBox.Show($"Invalid subtitle track index", MessageBoxType.Error);
                processfile = false;
            }
            else
            {
                dropdownSubtitles.SelectedIndex = indexsubtitles;
            }

            if (processfile)
            {
                BtnApplyClickedSub();
            }

        }
        btnApply.Enabled = true;
        btnApplyAll.Enabled = true;
    }

    protected void BtnApplyClickedSub()
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
            Website = new Uri("https://github.com/Ranft65/MkvDefaultTrackChanger/tree/feat/add-flags-and-command-line"),
            WebsiteLabel = "Github",
            ProgramDescription =
                "MkvDefaultTrackChanger is a small application to change the default subtitle and audio tracks in MKV video files. ",
            License = @"Copyright (C) 2021 Mike Moolenaar
MkvDefaultTrackChanger is licensed under the terms of the GNU General Public License version 3. A copy of this license can be obtained from <https://www.gnu.org/licenses/gpl-3.0.html>.",
            Developers = ["Mike Moolenaar, IDisposable, Ranft65"]
        };
        aboutDialog.ShowDialog(this);
    }

    private void HandleException(Exception ex)
    {
        var filePath = mkvFiles?[currentFileIndex]?.filePath;
        new ErrorForm(ex, filePath ?? "Unknown file", Icon).Show();
    }
    protected void BtnHelpClicked(object sender, EventArgs e)
    {
        var msgBox = new CustomMessageBox(
            "Command Line Usage Help",
            "For command line usage, use the following:\n\n" +
            "MkvDefaultTrackChanger  DefaultAudioTrack  DefaultSubtitleTrack  File(s)\n\n" +
            "Where\n\n" +
            "DefaultAudioTrack is the desired default audio track. The first audio track is track number one.\n\n" +
            "DefaultSubtitleTrack is the desired default subtitle track.  The first subtitle track is track number one.  Use zero for no default subtitle track.\n\n" +
            "File(s) is the list of files to modify.\n\n" +
            "Command Line Example:\n\n" +
            "MkvDefaultTrackChanger  2  1  file1.mkv  file2.mkv  file3.mkv\n\n" +
            "Please note that files are overwritten.  Only use this program on copies of the original files if you want to keep the original unmodifed files."
        );
        msgBox.ShowModal(this);
    }
    public class CustomMessageBox : Dialog<DialogResult>
    {
        public CustomMessageBox(string title, string message)
        {
            Title = title;
            //ClientSize = new Size(0,0); 
            Resizable = false;

            // Message label
            var label = new Label
            {
                Text = message,
                Wrap = WrapMode.Word,
                VerticalAlignment = VerticalAlignment.Center,
                //HorizontalAlignment = HorizontalAlignment.Center
            };

            // OK button
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) => Close(DialogResult.Ok);

            // Layout
            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Items =
            {
                label,
                new StackLayoutItem(okButton, HorizontalAlignment.Center)
            }
            };
        }

    }
}

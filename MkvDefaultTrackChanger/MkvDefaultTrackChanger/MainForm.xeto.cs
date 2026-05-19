using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    //Button btnHelp;

    MkvFilesContainer mkvContainer;
    OpenFileDialog fileDialog;
    private (string audio, string subtitles)? appliedConfig;

    public MainForm(string[] args) 
    {
        int indexaudio;
        int indexsubtitles;
        string[] filepaths;
        int i;

        Icon = Icon.FromResource("MkvDefaultTrackChanger.logo.ico");
        XamlReader.Load(this);

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
            RunCommandLine(indexaudio,indexsubtitles,filepaths);
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
        LoadFilesSub(filePaths);
    }

    private void LoadFilesSub(string[] filePaths)
    {
        //string[] filePaths = fileDialog.Filenames.ToArray();

        mkvContainer = new MkvFilesContainer(filePaths);
        if (mkvContainer.MkFilesRejected.Count > 0)
        {
            var sourceFile = Path.GetFileName(filePaths[0]);
            
            string rejectedFiles = Environment.NewLine + Environment.NewLine;
            mkvContainer.MkFilesRejected.ForEach((x) =>
            {
                rejectedFiles += $"- {Path.GetFileName(x.file.filePath)}: {x.error} {Environment.NewLine}{Environment.NewLine}";
            });
            MessageBox.Show($"The following files have different tracks or the order is different than {sourceFile}: {rejectedFiles}These files cannot be processed.", 
                MessageBoxType.Warning);
        }

        var lsSubtitleTracks = mkvContainer.GetSubtitleTracks();
        var lsAudioTracks = mkvContainer.GetAudioTracks();

        FillDropdown(dropdownSubtitles, lsSubtitleTracks);
        FillDropdown(dropdownAudio, lsAudioTracks);
        
        string files = filePaths.Length == 1 ? "file" : "files";
        lblFilesSelected.Text = $"{filePaths.Length} {files} selected";
    }

    private void RunCommandLine(int indexaudio, int indexsubtitles, string[] filePaths) 
    {
        LoadFilesSub(filePaths);
    
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

        mkvContainer.WriteChanges(track =>
        {
            string key = track.number.ToString();
            track.flagDefault = dropdownAudio.SelectedKey == key || dropdownSubtitles.SelectedKey == key;
        });

        //LoadFilesSub(filePaths);

        //appliedConfig = (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey);

        Environment.Exit(0);
        
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
            Logo = Icon.WithSize(100, 200),
            Website = new Uri("https://github.com/MikeMoolenaar/MkvDefaultTrackChanger"),
            WebsiteLabel = "Github",
            ProgramDescription =
                "MkvDefaultTrackChanger is a small application to change the default subtitle and audio tracks in MKV video files.",

            License = @"Copyright (C) 2021 Mike Moolenaar
MkvDefaultTrackChanger is licensed under the terms of the GNU General Public License version 3. A copy of this license can be obtained from <https://www.gnu.org/licenses/gpl-3.0.html>.",
            Developers = ["Mike Moolenaar"]
        };
        aboutDialog.ShowDialog(this);
        
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

    private void HandleException(Exception ex)
    {
        new ErrorForm(ex, mkvContainer?.ToString(), Icon).Show();
    }
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

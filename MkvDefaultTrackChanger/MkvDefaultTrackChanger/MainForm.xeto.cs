using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MatroskaLib;
using MatroskaLib.Types;
using Microsoft.VisualBasic;
using static System.Net.WebRequestMethods;

namespace MkvDefaultTrackChanger;

/* 
 *  Adding the option to set forced subtitles to be unforced caused the resulting mkv files to be unreadable.
 *  Therefore this option been disabled and its code to do has been commented out.
 *  
 *  Further investigation has shown that the changing of the forced flag in MatroskaWrite.cs seems to be the 
 *  cause of file corruption when the changes are made more than once.  Therefore the changing of the forced
 *  flag has been disabled for now until a solution to the file corruption is found.
 */

public class MainForm : Form
{
    Label lblFilesSelected;
    DropDown dropdownAudio;
    CheckBox donotmodifyaudiotracks;
    DropDown dropdownSubtitles;
    CheckBox donotmodifysubtitletracks;
    Button btnApply;
    Button btnApplyAll;
    Label lblStatus;
    Button btnPrevious;
    Button btnNext;
    Label lblCurrentFile;
    Label lblCurrentAudio;
    Label lblCurrentSubtitles;
    CheckBox disableAudioLanguageCheck;
    CheckBox disableAudioNameCheck;
    CheckBox disableSubtitleLanguageCheck;
    CheckBox disableSubtitleNameCheck;
    // CheckBox NoForcedSubtitles;
    Label Label1;
    Label Label2;

    bool commandlinemode = false;

    List <Track> lsSubtitleTracks;
    List <Track> lsAudioTracks;

    List <MkvFile> mkvFiles;
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

        // AutoSize = true;

#pragma warning disable CS8602
        disableAudioLanguageCheck.Checked = false;
        disableAudioNameCheck.Checked = false;
        disableSubtitleLanguageCheck.Checked = false;
        disableSubtitleNameCheck.Checked = false;
        // NoForcedSubtitles.Checked = false;

        appliedConfigs = new Dictionary<string, (string audio, string subtitles)>();

        if (args.Length >= 3)
        {
            commandlinemode = true;
            try
            {
                indexaudio = int.Parse(args[0]);
                if (indexaudio == -1) indexaudio = 0;
                indexaudio = indexaudio - 1;
                indexsubtitles = int.Parse(args[1]);
                List<string> templist = new List<string>();

                for (i = 2; i < args.Length; i++)
                {
                    if (args[i] == "-disable-all-track-sameness-checks")
                    {
                        disableAudioLanguageCheck.Checked = true;
                        disableAudioNameCheck.Checked = true;
                        disableSubtitleLanguageCheck.Checked = true;
                        disableSubtitleNameCheck.Checked = true;
                    }
                    else if (args[i] == "-disable-audio-language-check")
                    {
                        disableAudioLanguageCheck.Checked = true;
                    }
                    else if (args[i] == "-disable-audio-name-check")
                    {
                        disableAudioNameCheck.Checked = true;
                    }
                    else if (args[i] == "-disable-subtitle-language-check")
                    {
                        disableSubtitleLanguageCheck.Checked = true;
                    }
                    else if (args[i] == "-disable-subtitle-name-check")
                    {
                        disableSubtitleNameCheck.Checked = true;
                    }
                    //else if (args[i] == "-no-forced-subtitles")
                    //{
                    //    NoForcedSubtitles.Checked = true;
                    //    donotmodifysubtitletracks.Checked = false;
                    //}
                    else
                    {
                        templist.Add(args[i]);
                    }
                }
#pragma warning restore CS8602
                filepaths = templist.ToArray();
                RunCommandLine(indexaudio, indexsubtitles, filepaths);
            }
            catch
            {
                commandlinehelp();
                Environment.Exit(1);
            }
        }
        else if (args.Length > 0)
        {
            commandlinemode = true;
            commandlinehelp();
            Environment.Exit(1);
            // MessageBox.Show($"There must be at least three command line arguments if they are used.",MessageBoxType.Error);
            // return;
        }
        else
        {
            commandlinemode = false;
            fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileFilter("MKV files", "*.mkv"));
            fileDialog.MultiSelect = true;
        }

    }

    void RunCommandLine(int indexaudio, int indexsubtitles, string[] filepaths)
    {
        if (indexaudio == -1 && indexsubtitles == -1) return;

        LoadFilesSub(filepaths);
        currentFileIndex = 0;
        LoadCurrentFile();

        if (indexaudio == -1)
        {
            donotmodifyaudiotracks.Checked = true;
        }
        else
        {
            donotmodifyaudiotracks.Checked = false;
            if (indexaudio < 0 || indexaudio > dropdownAudio.Items.Count - 1)
            {
                Console.WriteLine("Error: Invalid audio track index");
                return;
            }
            dropdownAudio.SelectedIndex = indexaudio;
        }

        if (indexsubtitles == -1)
        {
            donotmodifysubtitletracks.Checked = true;
        }
        else
        {
            donotmodifysubtitletracks.Checked = false;
            if (indexsubtitles < 0 || indexsubtitles > dropdownSubtitles.Items.Count - 1)
            {
                Console.WriteLine("Invalid subtitle track index");
                return;
            }
            dropdownSubtitles.SelectedIndex = indexsubtitles;
        }

        ProcessAllFiles(true,indexaudio,indexsubtitles);
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

        if (lsSubtitleTracks != null) lsSubtitleTracks.Clear();
        lsSubtitleTracks = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.subtitle)
            .ToList();

        if (lsAudioTracks != null) lsAudioTracks.Clear();
        lsAudioTracks = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.audio)
            .ToList();

        lsSubtitleTracks.Insert(0, new TrackDisable());

        FillDropdown(dropdownSubtitles, lsSubtitleTracks);
        FillDropdown(dropdownAudio, lsAudioTracks);

        UpdateCurrentTrackLabels(lsAudioTracks, lsSubtitleTracks);

        lblCurrentFile.Text = $"File {currentFileIndex + 1} of {mkvFiles.Count}: {Path.GetFileName(currentFile.filePath)}";

        String text1;
        text1 = "Current file audio tracks:";
        for (int i = 0; i < dropdownAudio.Items.Count; i++)
        {
            text1 = text1 + Environment.NewLine + dropdownAudio.Items[i].Text;
        }
        Label1.Text = text1;

        String text2;
        text2 = "Current file subtitle tracks:";
        for (int i = 1; i < dropdownSubtitles.Items.Count; i++)
        {
            text2 = text2 + Environment.NewLine + dropdownSubtitles.Items[i].Text;
        }
        Label2.Text = text2;

        Height = 520 + Label1.Height + Label2.Height;

        int newwidth;
        newwidth = lblFilesSelected.Width;
        newwidth = Math.Max(newwidth, dropdownAudio.Width);
        newwidth = Math.Max(newwidth, donotmodifyaudiotracks.Width);
        newwidth = Math.Max(newwidth, dropdownSubtitles.Width);
        newwidth = Math.Max(newwidth, donotmodifysubtitletracks.Width);
        newwidth = Math.Max(newwidth, btnApply.Width);
        newwidth = Math.Max(newwidth, btnApplyAll.Width);
        newwidth = Math.Max(newwidth, lblStatus.Width);
        newwidth = Math.Max(newwidth, btnPrevious.Width); 
        newwidth = Math.Max(newwidth, btnNext.Width);
        newwidth = Math.Max(newwidth, lblCurrentFile.Width);
        newwidth = Math.Max(newwidth, lblCurrentAudio.Width);
        newwidth = Math.Max(newwidth, lblCurrentSubtitles.Width);
        newwidth = Math.Max(newwidth, disableAudioLanguageCheck.Width);
        newwidth = Math.Max(newwidth, disableAudioNameCheck.Width);
        newwidth = Math.Max(newwidth, disableSubtitleLanguageCheck.Width);
        newwidth = Math.Max(newwidth, disableSubtitleNameCheck.Width);
        newwidth = Math.Max(newwidth, Label1.Width);
        newwidth = Math.Max(newwidth, Label2.Width);
        Width = newwidth + 20;

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

        if (donotmodifyaudiotracks.Checked == true && donotmodifysubtitletracks.Checked == true)
        {
            return;
        }

        int indexaudio;
        int indexsubtitles;

        if (donotmodifyaudiotracks.Checked == true)
        {
            indexaudio = -1;
        }
        else
        {
            indexaudio = dropdownAudio.SelectedIndex;
        }
        if (donotmodifysubtitletracks.Checked == true)
        {
            indexsubtitles = -1;
        }
        else
        {
            indexsubtitles = dropdownSubtitles.SelectedIndex;
        }
        ProcessAllFiles(false,indexaudio,indexsubtitles);
    }
    protected void ProcessAllFiles(bool batchmode, int indexaudio, int indexsubtitles)
    {

        //string ProcessAllAudio;
        //string ProcessAllSubtitles;
        //string CurrentAudio;
        //string CurrentSubtitle;

        int indexaudiocurrentfile;
        int indexsubtitlescurrentfile;

        btnApply.Enabled = false;
        btnApplyAll.Enabled = false;

        List<string> errorlist = new List<string>();

        var currentFile = mkvFiles[currentFileIndex];

        MatroskaReader.LoadMediaInfoForFile(currentFile);

        var lsSubtitleTracksTest = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.subtitle)
            .ToList();
        var lsAudioTracksTest = currentFile.tracks
            .Where(x => x.type == TrackTypeEnum.audio)
            .ToList();

        lsSubtitleTracksTest.Insert(0, new TrackDisable());

        //if (indexaudio == -1)
        //{
        //    ProcessAllAudio = "";
        //}
        //else
        //{
        //    ProcessAllAudio = dropdownAudio.Items[indexaudio].Text;
        //}

        //if (indexsubtitles == -1)
        //{
        //    ProcessAllSubtitles = "";
        //}
        //else
        //{
        //    ProcessAllSubtitles = dropdownSubtitles.Items[indexsubtitles].Text;
        //}

        for (currentFileIndex = 0; currentFileIndex < mkvFiles.Count; currentFileIndex++)
        {
            bool processfile = true;

            try
            {
                LoadCurrentFile();
                UpdateNavigationButtons();

                indexaudiocurrentfile = dropdownAudio.SelectedIndex;
                indexsubtitlescurrentfile = dropdownSubtitles.SelectedIndex;

                //CurrentAudio = dropdownAudio.Items[dropdownAudio.SelectedIndex].Text;
                //CurrentSubtitle = dropdownSubtitles.Items[dropdownAudio.SelectedIndex].Text;

                if (indexaudio == -1)
                {
                    // do not look at the audio track index
                }
                else if (indexaudio < 0 || indexaudio > dropdownAudio.Items.Count - 1)
                {
                    errorlist.Add("Error: Invalid audio track index for " + mkvFiles[currentFileIndex].filePath);
                    processfile = false;
                }
                else
                {
                    dropdownAudio.SelectedIndex = indexaudio;
                }

                if (indexsubtitles == -1)
                {
                    // do not look at the subtitle track index
                }
                else if (indexsubtitles < 0 || indexsubtitles > dropdownSubtitles.Items.Count - 1)
                {
                    errorlist.Add("Error: Invalid subtitle track index for " + mkvFiles[currentFileIndex].filePath);
                    processfile = false;
                }
                else
                {
                    dropdownSubtitles.SelectedIndex = indexsubtitles;
                }

                if (indexaudio == indexaudiocurrentfile && indexsubtitles == indexsubtitlescurrentfile)
                {
                    // We could put processfile = false here but if so then the file status count would be wrong.
                    // Logic to not write files that do not have any default track values changed
                    // has been put in the process a single file function BtnApplyClickedSub.
                    // 
                }



                if (processfile)
                {
                    if (indexaudio == -1)
                    {
                        // do not check the audio track if it is not being changed
                    }
                    else
                    {
                        if (disableAudioLanguageCheck.Checked == false)
                        {
                            if (lsAudioTracks[indexaudio].language != lsAudioTracksTest[indexaudio].language)
                            {
                                processfile = false;
                                errorlist.Add("Error: Audio track language is not the same for " + mkvFiles[currentFileIndex].filePath);
                            }
                        }

                        if (disableAudioNameCheck.Checked == false)
                        {
                            if (lsAudioTracks[indexaudio].name != lsAudioTracksTest[indexaudio].name)
                            {
                                processfile = false;
                                errorlist.Add("Error: Audio track name is not the same for " + mkvFiles[currentFileIndex].filePath);
                            }
                        }

                    }

                    if (indexsubtitles == -1)
                    {
                        // do not check the subtitle track if it is not being changed
                    }
                    else
                    {
                        if (disableSubtitleLanguageCheck.Checked == false)
                        {
                            if (lsSubtitleTracks[indexsubtitles].language != lsSubtitleTracksTest[indexsubtitles].language)
                            {
                                processfile = false;
                                errorlist.Add("Error: Subtitle track language is not the same for " + mkvFiles[currentFileIndex].filePath);
                            }
                        }

                        if (disableSubtitleNameCheck.Checked == false)
                        {
                            if (lsSubtitleTracks[indexsubtitles].name != lsSubtitleTracksTest[indexsubtitles].name)
                            {
                                processfile = false;
                                errorlist.Add("Error: Subtitle track name is not the same for " + mkvFiles[currentFileIndex].filePath);
                            }
                        }
                    }

                }

                if (processfile)
                {
                    BtnApplyClickedSub();
                }
            }
            catch (Exception exception)
            {
                errorlist.Add("Error opening " + mkvFiles[currentFileIndex].filePath);
                HandleException(exception);
            }

        }
        currentFileIndex = mkvFiles.Count - 1;

        if (errorlist.Count > 0)
        {
            int completed = appliedConfigs.Count;
            int total = mkvFiles.Count;
            errorlist.Add($"{completed} of {total} files processed");

            string errormsg = string.Join(Environment.NewLine, errorlist);
            if (batchmode)
            {
                Console.WriteLine(errormsg);
            } 
            else
            {
                var msgbox = new CustomMessageBox("Error List",errormsg);
                msgbox.ShowModal(this);
            }
        }

        currentFileIndex = mkvFiles.Count - 1;
        btnApply.Enabled = true;
        btnApplyAll.Enabled = true;
    }

    protected void BtnApplyClickedSub()
    {
        bool changed = false;
        bool oldvalue;

        try
        {
            btnApply.Enabled = false;

            var currentFile = mkvFiles[currentFileIndex];

            if (donotmodifyaudiotracks.Checked == true && donotmodifysubtitletracks.Checked == true)
            {
                // Do nothing
            }
            else
            {
                currentFile.tracks.ForEach(track =>
                {
                    if (track.@type == TrackTypeEnum.audio && donotmodifyaudiotracks.Checked == true)
                    {
                        // do nothing since this is an audio track and audio tracks are not being modified
                    }
                    else if (track.@type == TrackTypeEnum.subtitle && donotmodifysubtitletracks.Checked == true)
                    {
                        // do nothing since this is an subtitle track and subtitle tracks are not being modified
                    }
                    else
                    {
                        string key = track.number.ToString();

                        oldvalue = track.flagDefault;

                        track.flagDefault = dropdownAudio.SelectedKey == key || dropdownSubtitles.SelectedKey == key;

                        if (track.flagDefault != oldvalue)
                        {
                            changed = true;
                        }

                        /*
                        if (track.@type == TrackTypeEnum.subtitle && track.flagForced == true && NoForcedSubtitles.Checked == true)
                        {
                            changed = true;   
                            // We do not change the forced flag here since it will mess up the writting of the mkv file later.
                            // Instead we force a write of the file.
                            // During any write of the file, all the forced subtitle flags are all set to false.
                            // track.flagForced = false;
                        }
                        */
                    }

                });

                if (changed) MatroskaWriter.WriteMkvFile(currentFile);
                
            }

            appliedConfigs[currentFile.filePath] = (dropdownAudio.SelectedKey, dropdownSubtitles.SelectedKey);

            mkvFiles[currentFileIndex] = MatroskaReader.ReadMkvFiles([currentFile.filePath])[0];
            LoadCurrentFile();

            int completed = appliedConfigs.Count;
            int total = mkvFiles.Count;
            lblStatus.Text = $"Saved! ({completed} of {total} files processed)";
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

    protected void NoForcedSubtitlesChanged(object sender, EventArgs e)
    {
        //if (NoForcedSubtitles.Checked == true)
        //{
        //    donotmodifyaudiotracks.Checked = false;
        //}
    }

    protected void HandleAbout(object sender, EventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            Logo = Icon.WithSize(100, 200),
            Website = new Uri("https://github.com/Ranft65/MkvDefaultTrackChanger/tree/feat/add-flags-and-command-line"),
            WebsiteLabel = "Github",
            Version = "1.4.0",
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
        commandlinehelp();
    }
    protected void commandlinehelp()
    {
        string helpstring;
        helpstring = 
            "For command line usage, use the following:\n\n" +
            "MkvDefaultTrackChanger  DefaultAudioTrack  DefaultSubtitleTrack <Options> File(s)\n\n" +
            "Where\n\n" +
            "DefaultAudioTrack is the desired default audio track. The first audio track is track number one.\n" +
            "Use -1 or 0 to not modify the default audio track\n\n" +
            "DefaultSubtitleTrack is the desired default subtitle track.  The first subtitle track is track number one.\n" +
            "Use -1 to not modifiy the default subtitle track\n" +
            "Use 0 for no default subtitle track.\n\n" +
            "The following options are optional and disable track sameness checking when multiple files are processed:\n" +
            "-disable-all-track-sameness-checks\n" +
            "-disable-audio-language-check\n" +
            "-disable-audio-name-check\n" + 
            "-disable-subtitle-language-check\n" +
            "-disable-subtitle-name-check\n\n" +
            // "The -no-forced-subtitles option will make all forced subtitles be unforced." +
            "File(s) is the list of files to modify.\n\n" +
            "Command Line Example:\n\n" +
            "MkvDefaultTrackChanger  2  1  file1.mkv  file2.mkv  file3.mkv\n\n" +
            "MkvDefaultTrackChanger -h or MkvDefaultTrackChanger -H will display this help.\n\n" +
            "The selected audio and subtitle tracks of the files must be the same.\n" +
            "Files with different tracks than the first file will not be processed unless checks are disabled.\n\n" +
            "Please note that files are overwritten.\n" +
            "Only use this program on copies of the original files if you want to keep the original unmodifed files.";

        if (commandlinemode)
        {
            Console.WriteLine(helpstring);
        }
        else
        {
            var msgBox = new CustomMessageBox("Command Line Usage Help", helpstring);
            msgBox.ShowModal(this);
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
}

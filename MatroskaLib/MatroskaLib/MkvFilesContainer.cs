using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaLib.Types;

namespace MatroskaLib;

public class MkvFilesContainer
{
    public readonly List<MkvFile> MkvFiles = new();
    public readonly List<(MkvFile file, string error)> MkFilesRejected = new();

    public MkvFilesContainer(string[] filePaths)
    {
        var files = MatroskaReader.ReadMkvFiles(filePaths);
        MkvFiles.Add(files[0]);
        for (int i = 1; i < files.Count; i++)
        {
            string? error = files[0].CompareToGetError(files[i]);
            if (error is null)
                MkvFiles.Add(files[i]);
            else
                MkFilesRejected.Add((files[i], error));
        }
    }

    public void WriteChanges(Action<Track> setDefaultIfSelected)
    {
        foreach (MkvFile file in MkvFiles)
        {
            file.tracks.ForEach(setDefaultIfSelected);
            MatroskaWriter.WriteMkvFile(file);
        }
    }

    public List<Track> GetSubtitleTracks()
    {
        var lsAudioTracks = MkvFiles.First()
            .tracks
            .Where(x => x.type == TrackTypeEnum.subtitle)
            .ToList();

        lsAudioTracks.Insert(0, new TrackDisable());
        return lsAudioTracks;
    }

    public List<Track> GetAudioTracks()
    {
        return MkvFiles.First()
            .tracks
            .Where(x => x.type == TrackTypeEnum.audio)
            .ToList();
    }

    public override string ToString() =>
        MkvFiles.Any() ? MkvFiles.First().ToString() : "No MKV files.";
}

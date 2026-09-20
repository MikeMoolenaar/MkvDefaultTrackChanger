using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaLib.Types;

namespace MatroskaLib;

public class MkvFilesContainer
{
    public readonly List<MkvFileGroup> Groups = new();

    public MkvFilesContainer(List<string> filePaths)
    {
        var files = MatroskaReader.ReadMkvFiles(filePaths);
        foreach (var file in files)
        {
            var group = Groups.FirstOrDefault(g => g.Reference.CompareToGetError(file) is null);
            if (group is not null)
                group.Files.Add(file);
            else
                Groups.Add(new MkvFileGroup(file));
        }
    }

    public void WriteChanges(MkvFileGroup group, Action<Track> setDefaultIfSelected)
    {
        foreach (MkvFile file in group.Files)
        {
            file.tracks.ForEach(setDefaultIfSelected);
            MatroskaWriter.WriteMkvFile(file);
        }
    }

    public List<Track> GetSubtitleTracks(MkvFileGroup group)
    {
        var subtitleTracks = group.Reference
            .tracks
            .Where(x => x.type == TrackTypeEnum.subtitle)
            .ToList();

        return [new TrackDisable(), ..subtitleTracks];
    }

    public List<Track> GetAudioTracks(MkvFileGroup group)
    {
        return group.Reference
            .tracks
            .Where(x => x.type == TrackTypeEnum.audio)
            .ToList();
    }

    public override string ToString() =>
        Groups.Count > 0 ? Groups[0].Reference.ToString() : "No MKV files.";
}

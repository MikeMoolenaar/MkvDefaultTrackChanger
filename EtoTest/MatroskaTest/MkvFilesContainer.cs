using System;
using System.Collections.Generic;
using System.Linq;

namespace MatroskaTest
{
    public class MkvFilesContainer
    {
        public List<MkvFile> lsMkvFiles = new();
        public List<MkvFile> lsMkFilesRejected = new();

        public MkvFilesContainer(string[] filePaths)
        {
            var lsMkvFiles = MatroskaLib.ReadMkvFiles(filePaths);
            this.lsMkvFiles.Add(lsMkvFiles[0]);
            for (int i = 1; i < lsMkvFiles.Count; i++)
            {
                if (lsMkvFiles[0].CompareTo(lsMkvFiles[i]) == 0)
                    this.lsMkvFiles.Add(lsMkvFiles[i]);
                else
                    this.lsMkFilesRejected.Add(lsMkvFiles[i]);
            }
        }

        public void WriteChanges(Action<Track> setDefaultIfSelected)
        {
            foreach (MkvFile file in lsMkvFiles)
            {
                file.tracks.ForEach(setDefaultIfSelected);
                MatroskaLib.WriteMkvFile(file.filePath, file.seekList, file.tracks, file.seekHeadCheckSum,
                    file.seekHeadCheckSum, file.voidPosition, file.endPosition, file.tracksPosition,
                    file.beginHeaderPosition);
            }
        }

        public List<Track> GetSubtitleTracks()
        {
            var lsAudioTracks = this.lsMkvFiles.First()
                .tracks
                .Where(x => x.type == TrackTypeEnum.subtitle)
                .ToList();
            lsAudioTracks.Insert(0, new TrackDisable());
            return lsAudioTracks;
        }

        public List<Track> GetAudioTracks()
        {
            return this.lsMkvFiles.First()
                .tracks
                .Where(x => x.type == TrackTypeEnum.audio)
                .ToList();
        }
    }
}
using System.Collections.Generic;
using System;
using System.Linq;

namespace MatroskaTest
{
    public class MkvFile : IComparable<MkvFile>
    {
        public string filePath;
        public List<Track> tracks;
        public int beginPosition;
        public int endPosition;
        public int tracksPosition;
        public int beginHeaderPosition;

        public MkvFile(string filePath, List<Track> tracks, int beginPosition, int endPosition, int tracksPosition,
            int beginHeaderPosition = 0)
        {
            this.filePath = filePath;
            this.tracks = tracks;
            this.beginPosition = beginPosition;
            this.endPosition = endPosition;
            this.tracksPosition = tracksPosition;
            this.beginHeaderPosition = beginHeaderPosition;
        }

        public int CompareTo(MkvFile other)
        {
            for (int i = 0; i < this.tracks.Count; i++)
            {
                var track = this.tracks[i];
                var trackOther = other.tracks.ElementAtOrDefault(i);
                if (trackOther is null || track.number != trackOther.number || track.language != trackOther.language)
                    return -1;
            }
            return 0;
        }
    }
}

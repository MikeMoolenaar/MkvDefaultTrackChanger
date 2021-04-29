using System.Collections.Generic;
using System;

namespace MatroskaTest
{
    public class MkvFile : IComparable<MkvFile>
    {
        public string filePath;
        public List<Track> tracks;
        public int voidPosition;
        public int tracksPosition;

        public MkvFile(string filePath, List<Track> tracks, int voidPosition, int tracksPosition)
        {
            this.filePath = filePath;
            this.tracks = tracks;
            this.voidPosition = voidPosition;
            this.tracksPosition = tracksPosition;
        }

        public int CompareTo(MkvFile other)
        {
            for (int i = 0; i < this.tracks.Count; i++)
            {
                var track = this.tracks[i];
                var trackOther = other.tracks[i];
                if (trackOther is null || track.number != trackOther.number || track.language != trackOther.language)
                    return -1;
            }
            return 0;
        }
    }
}

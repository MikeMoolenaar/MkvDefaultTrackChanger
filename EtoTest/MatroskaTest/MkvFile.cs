using System.Collections.Generic;
using System;
using System.Linq;
using Matroska;

namespace MatroskaTest
{
    public class MkvFile : IComparable<MkvFile>
    {
        public string filePath;
        public List<Track> tracks;
        public List<Seek> seekList;
        public int? seekHeadCheckSum;
        public int? tracksCheckSum;
        public int voidPosition;
        public int endPosition;
        public int tracksPosition;
        public int beginHeaderPosition;

        public MkvFile(string filePath, List<Track> tracks, List<Seek> seekList, int? seekHeadCheckSum,
            int? tracksCheckSum, int voidPosition,
            int endPosition,
            int tracksPosition,
            int beginHeaderPosition = 0)
        {
            this.filePath = filePath;
            this.tracks = tracks;
            this.seekList = seekList;
            this.seekHeadCheckSum = seekHeadCheckSum;
            this.tracksCheckSum = tracksCheckSum;
            this.voidPosition = voidPosition;
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

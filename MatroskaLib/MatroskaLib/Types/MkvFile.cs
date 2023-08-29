using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatroskaLib.Types;

public class MkvFile : IComparable<MkvFile>
{
    [JsonIgnore] public string filePath { get; }
    public List<Track> tracks { get; }
    public List<Seek> seekList { get; }
    public int? seekHeadCheckSum { get; }
    public int? tracksCheckSum { get; }
    public int voidPosition { get; }
    public int endPosition { get; }
    public int tracksPosition { get; }
    public int beginHeaderPosition { get; }

    public MkvFile(string filePath, List<Track> tracks, List<Seek> seekList, int? seekHeadCheckSum,
        int? tracksCheckSum, int voidPosition,
        int endPosition,
        int tracksPosition,
        int beginHeaderPosition)
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

    public int CompareTo(MkvFile? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        for (int i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            var trackOther = other.tracks.ElementAtOrDefault(i);
            if (trackOther is null || track.number != trackOther.number || track.language != trackOther.language)
                return -1;
        }

        return 0;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatroskaLib.Types;

public class MkvFile : IComparable<MkvFile>
{
    [JsonIgnore] public required string filePath { get; init; }
    public required List<Track> tracks { get; init; }
    public required List<Seek> seekList { get; init; }
    public required int? seekHeadCheckSum { get; init; }
    public required int? tracksCheckSum { get; init; }
    public required int voidPosition { get; init; }
    public required int endPosition { get; init; }
    public required int tracksPosition { get; init; }
    public required int beginHeaderPosition { get; init; }

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

    public override string ToString() => 
        JsonSerializer.Serialize(this, SourceGeneratedMkvFile.Default.MkvFile);
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MkvFile))]
internal partial class SourceGeneratedMkvFile : JsonSerializerContext { }

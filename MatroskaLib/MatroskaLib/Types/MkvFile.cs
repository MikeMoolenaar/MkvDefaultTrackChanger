using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatroskaLib.Types;

public record MkvFile
{
    public required string filePath { get; init; }
    public required List<Track> tracks { get; init; }
    public required List<Seek> seekList { get; init; }
    public required int? seekHeadCheckSum { get; init; }
    public required int? tracksCheckSum { get; init; }
    public required int voidPosition { get; init; }
    public required int endPosition { get; init; }
    public required int tracksPosition { get; init; }
    public required int beginHeaderPosition { get; init; }

    public string? CompareToGetError(MkvFile? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        for (int i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            var trackOther = other.tracks.ElementAtOrDefault(i);
            
            if (trackOther is null)
                return $"Track at index {i} does not exist, expected {track.type}{track.language}.";
            if (track.number != trackOther.number)
                return $"Track number {i} does not match. Expected {track.number}, got {trackOther.number}.";
            if (track.language != trackOther.language)
                return $"Track language {i} does not match. Expected {track.language}, got {trackOther.language}.";
        }

        return null;
    }

    public override string ToString() => 
        JsonSerializer.Serialize(this with { filePath = string.Empty }, SourceGeneratedMkvFile.Default.MkvFile);
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MkvFile))]
internal partial class SourceGeneratedMkvFile : JsonSerializerContext { }

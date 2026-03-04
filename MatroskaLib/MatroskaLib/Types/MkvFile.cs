using System.Collections.Generic;
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
    
    [JsonIgnore]
    public bool mediaInfoLoaded { get; set; }

    public override string ToString() => 
        JsonSerializer.Serialize(this with { filePath = string.Empty }, SourceGeneratedMkvFile.Default.MkvFile);
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MkvFile))]
internal partial class SourceGeneratedMkvFile : JsonSerializerContext { }

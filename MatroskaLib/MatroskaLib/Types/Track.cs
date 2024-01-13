using System.IO;
using System.Text.Json.Serialization;
using NEbml.Core;

namespace MatroskaLib.Types;

// https://www.matroska.org/technical/elements.html
public static class TrackElements
{
    public const ulong Entry = 0xae;
    public const ulong Number = 0xd7;
    public const ulong Type = 0x83;
    public const ulong Name = 0x536e;
    public const ulong FlagDefault = 0x88;
    public const ulong FlagForced = 0x55AA;
    public const ulong Language = 0x22b59c;
}

public static class MatroskaElements
{
    public const ulong SeekHead = 0x114D9B74;
    public const ulong SeekEntry = 0x4DBB;
    public const ulong SeekId = 0x53AB;
    public const ulong SeekPosition = 0x53AC;

    public const ulong SegmentInfo = 0x1549A966;
    public const ulong Tracks = 0x1654ae6b;
    public const ulong Segment = 0x18538067;

    public const ulong VoidElement = 0xEC;
    public const ulong CheckSum = 0xBF;
}

[JsonConverter(typeof(JsonStringEnumConverter<TrackTypeEnum>))]
public enum TrackTypeEnum
{
    video = 1,
    audio = 2,
    complex = 3,
    logo = 16,
    subtitle = 17,
    buttons = 18,
    control = 32,
    metadata = 33
}
public class Track
{
    private EbmlReader _reader { get; }
    public int trackLengthByteNumber { get; set; }

    public ulong number { get; set; }
    public bool flagDefault { get; set; }
    public int flagDefaultByteNumber { get; set; }
    public bool flagForced { get; set; }
    public int flagForcedByteNumber { get; set; }
    public int flagTypebytenumber { get; set; }
    public TrackTypeEnum type { get; set; }

    public string? name { get; set; } = string.Empty;
    public string language { get; set; } = "eng";

    public Track(EbmlReader reader) =>
        _reader = reader;

    public void ApplyElement(FileStream fileStream)
    {
        switch (_reader.ElementId.EncodedValue)
        {
            case TrackElements.Number:
                number = _reader.ReadUInt();
                break;
            case TrackElements.Name:
                name = _reader.ReadUtf();
                break;
            case TrackElements.FlagForced:
                flagForcedByteNumber = (int)fileStream.Position;
                flagForced = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagDefault:
                flagDefaultByteNumber = (int)fileStream.Position;
                flagDefault = _reader.ReadUInt() == 1;
                break;
            case TrackElements.Language:
                language = _reader.ReadUtf();
                break;
            case TrackElements.Type:
                flagTypebytenumber = (int)fileStream.Position;
                type = (TrackTypeEnum)_reader.ReadUInt();
                break;
        }
    }

    public override string ToString() =>
        $"{number} ({language}) default={flagDefault}\t forced={flagForced}\t {name}";

    public virtual string ToUiString() =>
        $"({language}) {name}";
}

public class TrackDisable : Track
{
    public TrackDisable() : base(null!) { }
    public override string ToString() => "Disable";
    public override string ToUiString() => "Disable";
}

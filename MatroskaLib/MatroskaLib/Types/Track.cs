using System;
using System.Collections.Generic;
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
    public const ulong CodecID = 0x86;
    public const ulong CodecPrivate = 0x63A2;
    public const ulong FlagDefault = 0x88;
    public const ulong FlagForced = 0x55AA;
    public const ulong FlagHearingImpaired = 0x55AB;
    public const ulong FlagVisualImpaired = 0x55AC;
    public const ulong FlagTextDescriptions = 0x55AD;
    public const ulong FlagOriginal = 0x55AE;
    public const ulong FlagCommentary = 0x55AF;

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
    public bool flagHearingImpaired { get; set; }
    public int flagHearingImpairedByteNumber { get; set; }
    public bool flagVisualImpaired { get; set; }
    public int flagVisualImpairedByteNumber { get; set; }
    public bool flagTextDescriptions { get; set; }
    public int flagTextDescriptionsByteNumber { get; set; }
    public bool flagOriginal { get; set; }
    public int flagOriginalByteNumber { get; set; }
    public bool flagCommentary { get; set; }
    public int flagCommentaryByteNumber { get; set; }

    public int flagTypebytenumber { get; set; }
    public TrackTypeEnum type { get; set; }

    public string? name { get; set; } = string.Empty;
    public string language { get; set; } = "eng";
    public string? codecId { get; set; } = string.Empty;

    [JsonIgnore]
    public string? detectedFormat { get; set; }
    
    [JsonIgnore]
    public double bitrate { get; set; }

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
            case TrackElements.CodecID:
                codecId = _reader.ReadAscii();
                break;
            case TrackElements.FlagForced:
                flagForcedByteNumber = (int)fileStream.Position;
                flagForced = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagDefault:
                flagDefaultByteNumber = (int)fileStream.Position;
                flagDefault = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagHearingImpaired:
                flagHearingImpairedByteNumber = (int)fileStream.Position;
                flagHearingImpaired = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagVisualImpaired:
                flagVisualImpairedByteNumber = (int)fileStream.Position;
                flagVisualImpaired = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagTextDescriptions:
                flagTextDescriptionsByteNumber = (int)fileStream.Position;
                flagTextDescriptions = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagOriginal:
                flagOriginalByteNumber = (int)fileStream.Position;
                flagOriginal = _reader.ReadUInt() == 1;
                break;
            case TrackElements.FlagCommentary:
                flagCommentaryByteNumber = (int)fileStream.Position;
                flagCommentary = _reader.ReadUInt() == 1;
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

    public virtual string ToUiString()
    {
        var parts = new List<string>();
        
        parts.Add($"#{number}:");
        
        if (!string.IsNullOrEmpty(name))
        {
            parts.Add(name!);
        }
        
        string codecDisplay = !string.IsNullOrEmpty(detectedFormat) 
            ? detectedFormat 
            : !string.IsNullOrEmpty(codecId) ? GetCodecDisplayName(codecId!) : string.Empty;
            
        if (!string.IsNullOrEmpty(codecDisplay))
        {
            parts.Add(codecDisplay);
        }
        
        if (bitrate > 0 && type == TrackTypeEnum.audio)
        {
            parts.Add(FormatBitrate(bitrate));
        }
        
        parts.Add($"({language})");
        
        var flags = new List<string>();
        if (flagDefault) flags.Add("Default");
        if (flagForced) flags.Add("Forced");
        if (flagHearingImpaired) flags.Add("Hearing Impaired");
        if (flagVisualImpaired) flags.Add("Visual Impaired");
        if (flagTextDescriptions) flags.Add("Text Descriptions");
        if (flagOriginal) flags.Add("Original");
        if (flagCommentary) flags.Add("Commentary");
        
        if (flags.Count > 0)
        {
            parts.Add($"[{string.Join(", ", flags)}]");
        }
        
        return string.Join(" ", parts);
    }
    
    private static string GetCodecDisplayName(string codecId)
    {      
        return codecId switch
        {
            "A_AAC" => "AAC",
            "A_AC3" => "Dolby Digital (AC-3)",
            "A_EAC3" => "Dolby Digital Plus (E-AC-3)",
            "A_DTS" => "DTS",
            "A_MPEG/L3" => "MP3",
            "A_MPEG/L2" => "MP2",
            "A_VORBIS" => "Vorbis",
            "A_FLAC" => "FLAC",
            "A_OPUS" => "Opus",
            "A_PCM/INT/LIT" => "PCM",
            "A_PCM/FLOAT/IEEE" => "PCM Float",
            "A_TRUEHD" => "Dolby TrueHD",
            "A_MLP" => "MLP",
            "A_WAVPACK4" => "WavPack",
            "A_ALAC" => "ALAC",
            "S_TEXT/UTF8" => "SRT",
            "S_TEXT/SSA" => "SSA",
            "S_TEXT/ASS" => "ASS",
            "S_TEXT/USF" => "USF",
            "S_TEXT/WEBVTT" => "WebVTT",
            "S_VOBSUB" => "VobSub",
            "S_HDMV/PGS" => "PGS",
            "S_KATE" => "Kate",
            "V_MPEG4/ISO/AVC" => "H.264",
            "V_MPEGH/ISO/HEVC" => "H.265",
            "V_VP8" => "VP8",
            "V_VP9" => "VP9",
            "V_AV1" => "AV1",
            "V_MPEG1" => "MPEG-1",
            "V_MPEG2" => "MPEG-2",
            "V_MPEG4/ISO/ASP" => "MPEG-4 ASP",
            _ => codecId?.Replace("A_", "").Replace("S_", "").Replace("V_", "") ?? ""
        };
    }
    
    private static string FormatBitrate(double bitrateInBps)
    {
        double kbps = bitrateInBps / 1000.0;
        
        if (kbps >= 1000)
        {
            double mbps = kbps / 1000.0;
            return $"{mbps:F1} Mbps";
        }
        
        return $"{Math.Round(kbps)} kbps";
    }
}

public class TrackDisable : Track
{
    public TrackDisable() : base(null!) { }
    public override string ToString() => "Disable";
    public override string ToUiString() => "Disable";
}


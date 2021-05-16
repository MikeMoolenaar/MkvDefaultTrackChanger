using System;
using NEbml.Core;
using System.IO;

namespace MatroskaTest
{
    // https://www.matroska.org/technical/elements.html
    public class TrackElements
    {
        public const ulong entry = 0xae;
        public const ulong number = 0xd7;
        public const ulong type = 0x83;
        public const ulong name = 0x536e;
        public const ulong flagDefault = 0x88;
        public const ulong flagForced = 0x55AA;
        public const ulong language = 0x22b59c;
    }

    public class MatroskaElements
    {
        public const ulong seekHead = 0x114D9B74;
        public const ulong seekEntry = 0x4DBB;
        public const ulong seekID = 0x53AB;
        public const ulong seekPosition = 0x53AC;

        public const ulong segmentInfo = 0x1549A966;
        public const ulong tracks = 0x1654ae6b;
        public const ulong segment = 0x18538067;
        public const ulong attachments = 0x1941a469;
        
        public const ulong voidElement = 0xEC;
        public const ulong checkSum = 0xBF;
    }

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
        private EbmlReader _reader;
        public int trackLengthByteNumber { get; set; }

        public ulong number { get; set; }
        public bool flagDefault { get; set; }
        public int flagDefaultByteNumber { get; set; }
        public bool flagForced { get; set; }
        public int flagForcedByteNumber { get; set; }
        public int flagTypebytenumber { get; set; }
        public TrackTypeEnum type { get; set; }
        
        public string? name { get; set; }
        public string language { get; set; } = "eng";

        public Track(EbmlReader reader)
        {
            this._reader = reader;
        }

        public void applyElement(FileStream datastream)
        {
            switch (this._reader.ElementId.EncodedValue)
            {
                case TrackElements.number:
                    this.trackLengthByteNumber = (int)datastream.Position - 3;
                    this.number = this._reader.ReadUInt();
                    break;
                case TrackElements.name:
                    this.name = this._reader.ReadUtf();
                    break;
                case TrackElements.flagForced:
                    this.flagForcedByteNumber = (int)datastream.Position;
                    this.flagForced = this._reader.ReadUInt() == 1;
                    break;
                case TrackElements.flagDefault:
                    this.flagDefaultByteNumber = (int)datastream.Position;
                    this.flagDefault = this._reader.ReadUInt() == 1;
                    break;
                case TrackElements.language:
                    this.language = this._reader.ReadUtf();
                    break;
                case TrackElements.type:
                    this.flagTypebytenumber = (int)datastream.Position;
                    this.type = (TrackTypeEnum)this._reader.ReadUInt();
                    break;
            }
        }

        public override string ToString()
        {
            return $"{this.number} ({this.language ?? "eng"}) default={this.flagDefault}\t forced={this.flagForced}\t {this.name}";
        }

        public virtual string ToUiString()
        {
            return $"({this.language ?? "eng"}) {this.name}";
        }
    }

    public class TrackDisable : Track
    {
        public TrackDisable() : base(null) { }
        public override string ToString() => "Disable";
        public override string ToUiString() => "Disable";
    }
}

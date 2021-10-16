using System.IO;
using NEbml.Core;

namespace MatroskaLib
{
    public class Seek
    {
        private readonly EbmlReader _reader;
        
        
        public ulong seekID { get; private set; }
        public ulong seekPosition { get; private set; }
        public int seekPositionByteNumber { get; private set; }

        public Seek(EbmlReader reader)
        {
            this._reader = reader;
        }
        
        public void applyElement(FileStream datastream)
        {
            if (this._reader.ElementId.EncodedValue == MatroskaElements.seekID)
            {
                this.seekID = this._reader.ReadUInt();
            }
            else if (this._reader.ElementId.EncodedValue == MatroskaElements.seekPosition)
            {
                this.seekPositionByteNumber = (int)datastream.Position;
                this.seekPosition = this._reader.ReadUInt();
            }
        }
    }
}
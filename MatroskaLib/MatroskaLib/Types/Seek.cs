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

        public Seek(EbmlReader reader) => 
            _reader = reader;

        public void ApplyElement(FileStream fileStream)
        {
            if (_reader.ElementId.EncodedValue == MatroskaElements.seekID)
            {
                seekID = _reader.ReadUInt();
            }
            else if (_reader.ElementId.EncodedValue == MatroskaElements.seekPosition)
            {
                seekPositionByteNumber = (int)fileStream.Position;
                seekPosition = _reader.ReadUInt();
            }
        }
    }
}
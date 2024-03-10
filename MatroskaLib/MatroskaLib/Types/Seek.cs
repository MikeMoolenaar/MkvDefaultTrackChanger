using System.IO;
using NEbml.Core;

namespace MatroskaLib.Types;

public class Seek
{
    private readonly EbmlReader _reader;
    public ulong seekId { get; private set; }
    public ulong seekPosition { get; private set; }
    public int seekPositionByteNumber { get; private set; }
    public int elementLength { get; private set; }

    public Seek(EbmlReader reader) =>
        _reader = reader;

    public void ApplyElement(FileStream fileStream)
    {
        if (_reader.ElementId.EncodedValue == MatroskaElements.SeekId)
        {
            seekId = _reader.ReadUInt();
        }
        else if (_reader.ElementId.EncodedValue == MatroskaElements.SeekPosition)
        {
            seekPositionByteNumber = (int)fileStream.Position;
            seekPosition = _reader.ReadUInt();
            elementLength = (int)_reader.ElementSize;
        }
    }
}

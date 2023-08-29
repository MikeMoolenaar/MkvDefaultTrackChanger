using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatroskaLib.Types;
using NEbml.Core;
using NEbml.Matroska;

namespace MatroskaLib;

public static class MatroskaReader
{
    public static List<MkvFile> ReadMkvFiles(string[] filePaths)
    {
        var mkvFiles = new List<MkvFile>();
        foreach (var filePath in filePaths)
        {
            var trackList = new List<Track>();
            var seekList = new List<Seek>();

            using var fileStream = File.Open(filePath, FileMode.Open);
            var reader = new EbmlReader(fileStream);

            int? seekHeadCheckSum = _ReadSeekHead(reader, fileStream, seekList);
            int voidPosition = _LocateVoidElement(reader);

            (int tracksPosition, int beginHeaderPosition) = _DetermineTracksPosition(ref reader, fileStream, seekList, voidPosition);
            int? tracksCheckSum = _ReadTracks(reader, fileStream, trackList);

            int endPosition = _DetermineEndPosition(reader, beginHeaderPosition, voidPosition);
            
            mkvFiles.Add(new MkvFile(filePath, trackList, seekList, seekHeadCheckSum, tracksCheckSum, voidPosition, endPosition,
                tracksPosition, beginHeaderPosition));
        }

        return mkvFiles;
    }

    private static int? _ReadSeekHead(EbmlReader reader, FileStream fileStream, List<Seek> seekList)
    {
        int? seekHeadCheckSum = null;

        reader.LocateElement(MatroskaElements.Segment);
        reader.LocateElement(MatroskaElements.SeekHead);

        while (reader.ReadNext())
        {
            if (reader.ElementId.EncodedValue == MatroskaElements.CheckSum)
            {
                seekHeadCheckSum = (int)reader.ElementPosition;
            }
            else if (reader.ElementId.EncodedValue == MatroskaElements.SeekEntry)
            {
                var seek = new Seek(reader);

                reader.EnterContainer();
                while (reader.ReadNext())
                {
                    seek.ApplyElement(fileStream);
                }
                reader.LeaveContainer();

                if (seekList.All(x => x.seekId != seek.seekId))
                    seekList.Add(seek);
            }
        }
        reader.LeaveContainer();

        return seekHeadCheckSum;
    }

    private static int _LocateVoidElement(EbmlReader reader)
    {
        reader.LocateElement(MatroskaElements.VoidElement);
        var voidPosition = (int)reader.ElementPosition;
        reader.LeaveContainer();
        return voidPosition;
    }

    private static (int tracksPosition, int beginHeaderPosition) _DetermineTracksPosition(ref EbmlReader reader, FileStream fileStream, List<Seek> seekList, int voidPosition)
    {
        int beginHeaderPosition = 0;

        if (seekList.FirstOrDefault(x => x.seekId == MatroskaElements.Tracks)?.seekPosition < (ulong)voidPosition)
        {
            // Void is after track element, read file again and go to tracks element
            fileStream.Position = 0;
            reader = new EbmlReader(fileStream);
            reader.LocateElement(MatroskaElements.Segment);
            reader.LocateElement(MatroskaElements.Tracks);
        }
        else
        {
            reader.ReadNext();
            beginHeaderPosition = (int)reader.ElementPosition;

            if (reader.ElementId.EncodedValue != MatroskaElements.Tracks)
                reader.LocateElement(MatroskaElements.Tracks);
            else
                reader.EnterContainer();
        }
        return ((int)fileStream.Position, beginHeaderPosition);
    }

    private static int? _ReadTracks(EbmlReader reader, FileStream fileStream, List<Track> trackList)
    {
        int? tracksCheckSum = null;

        while (reader.ReadNext())
        {
            if (reader.ElementId.EncodedValue == MatroskaElements.CheckSum)
            {
                tracksCheckSum = (int)reader.ElementPosition;
            }
            else if (reader.ElementId.EncodedValue == TrackElements.Entry)
            {
                var track = new Track(reader)
                {
                    trackLengthByteNumber = (int)fileStream.Position
                };

                reader.EnterContainer();
                while (reader.ReadNext())
                {
                    track.ApplyElement(fileStream);
                }
                reader.LeaveContainer();

                trackList.Add(track);
            }
        }
        reader.LeaveContainer();

        return tracksCheckSum;
    }

    private static int _DetermineEndPosition(EbmlReader reader, int beginHeaderPosition, int voidPosition)
    {
        reader.ReadNext();
        return beginHeaderPosition != 0 ?
            (int)reader.ElementPosition :
            voidPosition + 9;
    }
}

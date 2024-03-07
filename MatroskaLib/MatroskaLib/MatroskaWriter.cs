using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatroskaLib.Types;

namespace MatroskaLib;

public static class MatroskaWriter
{
    public static void WriteMkvFile(MkvFile mkfFile, bool dryRun = false)
    {
        using var dataStream = File.Open(mkfFile.filePath, FileMode.Open);
        dataStream.Seek(0, SeekOrigin.Begin);

        byte[] bytes = new byte[mkfFile.endPosition];
        dataStream.Read(bytes, 0, bytes.Length);
        List<byte> lsBytes = new List<byte>(bytes);

        int offset = 0;
        _ChangeTrackElements(mkfFile.tracks, lsBytes, ref offset);
        ByteHelper.ChangeLength(lsBytes, mkfFile.tracksPosition, MatroskaElements.Tracks, offset);

        _ChangeVoidLengthAndHeaders(mkfFile.seekList, mkfFile.seekHeadCheckSum, mkfFile.tracksCheckSum, mkfFile.voidPosition, mkfFile.beginHeaderPosition,
            offset, lsBytes);

        // Write modified changes to file
        if (dryRun) return;
        dataStream.Seek(0, SeekOrigin.Begin);
        dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
    }

    private static void _ChangeTrackElements(List<Track> tracks, List<byte> lsBytes, ref int offset)
    {
        foreach (Track t in tracks.Where(x => x.type is TrackTypeEnum.audio or TrackTypeEnum.subtitle))
        {
            byte defaultFlag = (byte)(t.flagDefault ? 0x1 : 0x0);
            if (t.flagDefaultByteNumber != 0)
            {
                // Default flag is present, change it
                lsBytes[offset + t.flagDefaultByteNumber] = defaultFlag;
            }
            else if (t.flagTypebytenumber != 0)
            {
                // Default flag is not present, add it after the track entry element
                ByteHelper.ChangeLength(lsBytes, offset + t.trackLengthByteNumber, TrackElements.Entry, 3);
                lsBytes.InsertRange(offset + t.flagTypebytenumber + 1,
                    new byte[] { 0x88, 0x81, defaultFlag });
                offset += 3;
            }

            // Set forced flag to 0 if present
            if (t.flagForcedByteNumber != 0)
            {
                int correction = t.flagForcedByteNumber < t.flagTypebytenumber ? 3 : 0;
                lsBytes[offset + t.flagForcedByteNumber - correction] = 0x0;
            }
        }
    }

    private static void _ChangeVoidLengthAndHeaders(List<Seek> seekList, int? seekHeadCheckSum, int? tracksCheckSum,
        int voidPosition, int beginHeaderPosition, int offset, List<byte> lsBytes)
    {
        if (beginHeaderPosition != 0 && offset != 0)
        {
            // Void is before the header, change the length of the void element
            lsBytes.RemoveRange(beginHeaderPosition - offset, offset);
            ByteHelper.ChangeVoidLength(lsBytes, voidPosition);

            // In the Segment Information part, change the position of the tracks and segmentinfo
            //  elements as they have been changed.
            foreach (var s in seekList.Where(s =>
                         s.seekId is MatroskaElements.Tracks or MatroskaElements.SegmentInfo))
            {
                List<byte> lsNewBytes = ByteHelper.ToBytes(s.seekPosition - (ulong)offset);
                if (lsNewBytes.Count > s.elementLength)
                    throw new InvalidOperationException($"New seekPosition bytes are bigger than the old one. Trying to fit {lsNewBytes.Count} bytes into {s.elementLength} bytes");
                if (lsNewBytes.Count < s.elementLength)
                {
                    // The new seekPosition is smaller than the old one, add padding
                    lsNewBytes.AddRange(new byte[s.elementLength - lsNewBytes.Count]);
                }

                lsBytes.RemoveRange(s.seekPositionByteNumber, lsNewBytes.Count);
                lsBytes.InsertRange(s.seekPositionByteNumber, lsNewBytes);
            }

            // Remove checksums
            if (seekHeadCheckSum.HasValue)
                ByteHelper.ReplaceHashWithVoid(lsBytes, seekHeadCheckSum.Value);
            if (tracksCheckSum.HasValue)
                ByteHelper.ReplaceHashWithVoid(lsBytes, tracksCheckSum.Value - offset);
        }
        else if (beginHeaderPosition == 0 && offset != 0)
        {
            // Void is after the header, change the length of the void element
            var lsVoidLength = lsBytes.GetRange(voidPosition + offset + 1, 8);
            ByteHelper.ChangeLength(lsBytes, voidPosition + offset + 1, lsVoidLength, offset * -1);
        }
    }
}

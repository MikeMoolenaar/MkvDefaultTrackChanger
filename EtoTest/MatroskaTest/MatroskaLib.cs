using System.Collections.Generic;
using System.Linq;
using System.IO;
using NEbml.Core;
using NEbml.Matroska;
using System;
using Matroska;

namespace MatroskaTest
{
    // MKV header structure https://hybridego.net/entry/Matroska-Header
    // https://github.com/OlegZee/NEbml/blob/master/Src/MkvTitleEdit/Matroska/SegmentInfoUpdater.cs
    public static class MatroskaLib
    {
        public static List<MkvFile> ReadMkvFiles(string[] filePaths)
        {
            var lsMkvFiles = new List<MkvFile>();
            foreach (var filePath in filePaths)
            {
                List<Track> trackList = new List<Track>();
                List<Seek> seekList = new List<Seek>();
                using (var dataStream = File.Open(filePath, FileMode.Open))
                {
                    var reader = new EbmlReader(dataStream);

                    reader.LocateElement(MatroskaElements.segment);

                    reader.LocateElement(MatroskaElements.seekHead);
                    int? seekHeadCheckSum = null;
                    int? tracksCheckSum = null;

                    while (reader.ReadNext())
                    {
                        if (reader.ElementId.EncodedValue == MatroskaElements.checkSum)
                        {
                            seekHeadCheckSum = (int) reader.ElementPosition;
                        }
                        else if (reader.ElementId.EncodedValue == MatroskaElements.seekEntry)
                        {
                            var seek = new Seek(reader);

                            reader.EnterContainer();
                            while (reader.ReadNext())
                            {
                                seek.applyElement(dataStream);
                            }

                            reader.LeaveContainer();

                            if (seekList.All(x => x.seekID != seek.seekID))
                                seekList.Add(seek);
                        }
                    }

                    reader.LeaveContainer();

                    reader.LocateElement(MatroskaElements.voidElement);
                    int voidPosition = (int) reader.ElementPosition;
                    reader.LeaveContainer();

                    int beginHeaderPosition = 0;
                    int tracksPosition = 0;
                    if (seekList.First(x => x.seekID == MatroskaElements.tracks).seekPosition < (ulong) voidPosition)
                    {
                        // Void is after track element, read file again and go to tracks element
                        dataStream.Position = 0;
                        reader = new EbmlReader(dataStream);
                        reader.LocateElement(MatroskaElements.segment);
                        reader.LocateElement(MatroskaElements.tracks);
                        tracksPosition = (int) dataStream.Position;
                    }
                    else
                    {
                        reader.ReadNext();
                        beginHeaderPosition = (int) reader.ElementPosition;

                        reader.LocateElement(MatroskaElements.tracks);
                        tracksPosition = (int) dataStream.Position;
                    }
                    
                    // Loop over tracks
                    while (reader.ReadNext())
                    {
                        if (reader.ElementId.EncodedValue == MatroskaElements.checkSum)
                        {
                            tracksCheckSum = (int) reader.ElementPosition;
                        }
                        else if (reader.ElementId.EncodedValue == TrackElements.entry)
                        {
                            var track = new Track(reader);
                            track.trackLengthByteNumber = (int)dataStream.Position;
                            // Loop over track element and put them in track
                            reader.EnterContainer();
                            while (reader.ReadNext())
                            {
                                track.applyElement(dataStream);
                            }

                            reader.LeaveContainer();

                            trackList.Add(track);
                        }
                    }
                    

                    reader.LeaveContainer();
                    reader.ReadNext();
                    int endPosition = beginHeaderPosition != 0 ? (int) reader.ElementPosition : voidPosition + 8 + 1;

                    // TODO way to many parameters, put in seperate object
                    lsMkvFiles.Add(new MkvFile(filePath, trackList, seekList, seekHeadCheckSum, tracksCheckSum, voidPosition, endPosition,
                        tracksPosition, beginHeaderPosition));
                }
            }

            return lsMkvFiles;
        }

        public static void WriteMkvFile(string filePath, List<Seek> seekList, List<Track> trackList,
            int? seekHeadCheckSum, int? tracksCheckSum, int voidPosition,
            int endPosition, int tracksPosition, int beginHeaderPosition)
        {
            using (var dataStream = File.Open(filePath, FileMode.Open))
            {
                byte[] bytes = new byte[endPosition];

                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Read(bytes, 0, bytes.Length);
                List<byte> lsBytes = new List<byte>(bytes);

                int offset = 0;
                foreach (Track t in trackList
                        .Where(x => x.type == TrackTypeEnum.audio || x.type == TrackTypeEnum.subtitle)
                        .Where(x => x is not TrackDisable) // Maybe isn't needed?
                )
                {
                    byte defaultFlag = (byte) (t.flagDefault == true ? 0x1 : 0x0);
                    if (t.flagDefaultByteNumber != 0)
                    {
                        lsBytes[offset + t.flagDefaultByteNumber] = defaultFlag;
                    }
                    else if (t.flagTypebytenumber != 0)
                    {
                        // Change length of TrackEntry element 0xAE
                        ByteHelper.ChangeLength(lsBytes, offset + t.trackLengthByteNumber, TrackElements.entry, 3);

                        lsBytes.InsertRange(offset + t.flagTypebytenumber + 1,
                            new byte[] {0x88, 0x81, defaultFlag});
                        offset += 3;
                    }

                    if (t.flagForcedByteNumber != 0)
                    {
                        lsBytes[offset + t.flagForcedByteNumber] = 0x0;
                    }
                }

                // Change length of Tracks element
                ByteHelper.ChangeLength(lsBytes, tracksPosition, MatroskaElements.tracks, offset);

                if (beginHeaderPosition != 0 && offset != 0)
                {
                    // Remove some void data so it fits in the file without re-writing the entire file
                    lsBytes.RemoveRange(beginHeaderPosition - offset, offset);
                    ByteHelper.ChangeVoidLength(lsBytes, voidPosition);

                    // In the Segment Information part, change the position of the tracks and segmentinfo
                    //  elements as they have been changed.
                    foreach (var s in seekList.Where(s =>
                        s.seekID == MatroskaElements.tracks || s.seekID == MatroskaElements.segmentInfo))
                    {
                        int desiredLength = Convert.ToInt32(lsBytes[s.seekPositionByteNumber - 1] - 0x80);
                        List<byte> lsNewBytes = ByteHelper.ToBytes(s.seekPosition - (ulong)offset);
                        if (desiredLength != lsNewBytes.Count) throw new Exception("Doesn't fit in array :(");

                        lsBytes.RemoveRange(s.seekPositionByteNumber, lsNewBytes.Count);
                        lsBytes.InsertRange(s.seekPositionByteNumber, lsNewBytes);
                    }

                    if (seekHeadCheckSum.HasValue) 
                        ByteHelper.ReplaceHashWithVoid(lsBytes, seekHeadCheckSum.Value);
                    if (tracksCheckSum.HasValue) 
                        ByteHelper.ReplaceHashWithVoid(lsBytes, tracksCheckSum.Value - offset);

                }
                else if (beginHeaderPosition == 0 && offset != 0)
                {
                    var lsVoidLength = lsBytes.GetRange(voidPosition + offset + 1, 8);
                    ByteHelper.ChangeLength(lsBytes, voidPosition + offset + 1, lsVoidLength, offset*-1);
                }
                
                // Write modiefied changes to file
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
        }
    }
}
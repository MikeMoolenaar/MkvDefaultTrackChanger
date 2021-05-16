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

                            seekList.Add(seek);
                        }
                    }

                    reader.LeaveContainer();

                    reader.LocateElement(MatroskaElements.voidElement);
                    int voidPosition = (int) reader.ElementPosition;
                    reader.LeaveContainer();

                    reader.ReadNext();
                    int beginHeaderPosition = (int) reader.ElementPosition;

                    reader.LocateElement(MatroskaElements.tracks);
                    int tracksPosition = (int) dataStream.Position - 1;

                    // Loop over tracks
                    try
                    {
                        while (reader.ReadNext())
                        {
                            if (reader.ElementId.EncodedValue == MatroskaElements.checkSum)
                            {
                                tracksCheckSum = (int) reader.ElementPosition;
                            }
                            else if (reader.ElementId.EncodedValue == TrackElements.entry)
                            {
                                var track = new Track(reader);

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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    reader.LeaveContainer();

                    reader.LocateElement(MatroskaElements.attachments);
                    int endPosition = (int) reader.ElementPosition - 1;

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
                        lsBytes[offset + t.trackLengthByteNumber] += 0x3; // TODO make this dynamic in seperate function, should generate 2 bytes if the size is 2
                        if (lsBytes[offset + t.trackLengthByteNumber - 2] == 0x0)
                        {
                            // TODO This won't work if Length is not 8 bytes in total, can be removed if a seperate function is made
                            lsBytes[offset + t.trackLengthByteNumber - 1] = 0x0;
                        }

                        lsBytes.InsertRange(offset + t.flagTypebytenumber + 1,
                            new byte[] {0x88, 0x81, defaultFlag});
                        offset += 3;
                    }

                    if (t.flagForcedByteNumber != 0)
                    {
                        lsBytes[offset + t.flagForcedByteNumber] = 0x0;
                    }
                }

                // Change length of Tracks element TODO: make dynamic
                lsBytes[tracksPosition] += Convert.ToByte(offset);

                // Remove some void data so it fits in the file without re-writing the entire file
                lsBytes.RemoveRange(beginHeaderPosition - offset, offset);

                // Change length of the void element, because some of that space has been used 
                ChangeVoidLength(lsBytes, voidPosition);

                // In the Segment Information part, change the position of the tracks and segmentinfo
                //  elements as they have been changed.
                foreach (var s in seekList.Where(s =>
                    s.seekID == MatroskaElements.tracks || s.seekID == MatroskaElements.segmentInfo))
                {
                    int desiredLength = Convert.ToInt32(lsBytes[s.seekPositionByteNumber - 1] - 0x80);
                    byte[] newBytes = BitConverter.GetBytes(Convert.ToInt32(s.seekPosition) - offset);
                    Array.Resize(ref newBytes, desiredLength);
                    List<byte> lsNewBytes = newBytes.Reverse().ToList();
                    
                    lsBytes.RemoveRange(s.seekPositionByteNumber, lsNewBytes.Count);
                    lsBytes.InsertRange(s.seekPositionByteNumber, lsNewBytes);
                }

                if (seekHeadCheckSum.HasValue) 
                    ReplaceHashWithVoid(lsBytes, seekHeadCheckSum.Value);
                if (tracksCheckSum.HasValue) 
                    ReplaceHashWithVoid(lsBytes, tracksCheckSum.Value - offset);

                // Write modiefied changes to file
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
        }

        private static void ReplaceHashWithVoid(List<byte> lsBytes, int checkSumPosition)
        {
            lsBytes.RemoveRange(checkSumPosition, 6);
            lsBytes.InsertRange(checkSumPosition, new byte[] { 0xEC, 0x84, 0x0, 0x0, 0x0, 0x0 });
        }

        private static void ChangeVoidLength(List<byte> lsBytes, int voidPosition)
        {
            // Remove existing length
            lsBytes.RemoveRange(voidPosition + 1, 8);
            uint zeroCount = 0;

            // Replace with new length bytes
            for (var i = voidPosition + 1; i < lsBytes.Count; i++)
            {
                if (lsBytes[i] != 0x0) break;
                zeroCount++;
            }

            List<byte> voidLengthBytes = BitConverter.GetBytes(zeroCount | 1UL << (7*8)).Reverse().ToList();
            lsBytes.InsertRange(voidPosition + 1, voidLengthBytes);
        }
    }
}
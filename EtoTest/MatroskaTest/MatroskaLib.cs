using System.Collections.Generic;
using System.Linq;
using System.IO;
using NEbml.Core;
using NEbml.Matroska;
using System;

namespace MatroskaTest
{
    // MKV header structure https://hybridego.net/entry/Matroska-Header
    // https://github.com/OlegZee/NEbml/blob/master/Src/MkvTitleEdit/Matroska/SegmentInfoUpdater.cs
    public class MatroskaLib
    {
        public static List<MkvFile> ReadMkvFiles(string[] filePaths)
        {
            var lsMkvFiles = new List<MkvFile>();
            foreach(var filePath in filePaths)
            {
                int voidPosition = 0;
                List<Track> trackList = new List<Track>();
                using (var dataStream = File.Open(filePath, FileMode.Open))
                {
                    var reader = new EbmlReader(dataStream);
                    //reader.ReadNext();

                    if (reader.LocateElement(MatroskaElements.segment))
                    {
                        reader.EnterContainer();
                        if (reader.LocateElement(MatroskaElements.tracks))
                        {
                            reader.EnterContainer();
                            while (reader.ReadNext())
                            {
                                if (reader.ElementId.EncodedValue == TrackElements.entry)
                                {
                                    var track = new Track(reader);

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
                        reader.LeaveContainer();
                        if (reader.LocateElement(MatroskaElements.voidElement))
                        {
                            voidPosition = (int)dataStream.Position;
                        }
                    }
                }
                lsMkvFiles.Add(new MkvFile(filePath, trackList, voidPosition));
            }
           

            return lsMkvFiles;
        }

        public static void WriteMkvFile(string filePath, List<Track> trackList, int voidPosition)
        {
            using (var memoryStream = new MemoryStream())
            using (var dataStream = File.Open(filePath, FileMode.Open))
            {
                // Copy file contents to memory
                dataStream.CopyTo(memoryStream);
                List<byte> lsBytes = new List<byte>(memoryStream.ToArray());

                int offset = 0;

                trackList
                    .Where(x => x.type == TrackTypeEnum.audio || x.type == TrackTypeEnum.subtitle)
                    .Where(x => x is not TrackDisable) // Maybe isn't needed?
                    .ToList()
                    .ForEach((Track t) =>
                    {
                        byte defaultFlag = (byte)(t.flagDefault == true ? 0x1 : 0x0);
                        if (t.flagDefaultByteNumber != 0)
                        {
                            lsBytes[offset + t.flagDefaultByteNumber] = defaultFlag;
                        }
                        else if (t.flagTypebytenumber != 0)
                        {
                            // Change length of TrackEntry element 0xAE
                            lsBytes[offset + t.trackLengthByteNumber] += 0x3;

                            lsBytes.InsertRange(offset + t.flagTypebytenumber + 1, new byte[] { 0x88, 0x81, defaultFlag });
                            offset += 3;
                        }

                        if (t.flagForcedByteNumber != 0)
                        {
                            lsBytes[offset + t.flagForcedByteNumber] = 0x0;
                        }
                    });

                // Change length of Tracks element at 0x1654AE6B
                lsBytes[trackList[0].trackLengthByteNumber - 2] += Convert.ToByte(offset);

                // (change padding) Change void length after Tracks element and remove those bytes
                lsBytes[offset + voidPosition - 1] -= Convert.ToByte(offset);
                lsBytes.RemoveRange(offset + voidPosition, offset);

                // Write modiefied changes to file
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
        }
    }
}

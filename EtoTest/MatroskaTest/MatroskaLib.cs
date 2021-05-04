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
            foreach (var filePath in filePaths)
            {
                List<Track> trackList = new List<Track>();
                using (var dataStream = File.Open(filePath, FileMode.Open))
                {
                    var reader = new EbmlReader(dataStream);

                    reader.LocateElement(MatroskaElements.segment);
                    reader.LocateElement(MatroskaElements.tracks);
                    int tracksPosition = (int) dataStream.Position;
                    
                    // Loop over tracks
                    while (reader.ReadNext())
                    {
                        if (reader.ElementId.EncodedValue == TrackElements.entry)
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
                    reader.LeaveContainer();
                    int voidPosition = 0;
                    
                    // Store position of void element, this is the padding and may be adjusted
                    //  when changing the track default flag
                    try
                    {
                        reader.LocateElement(MatroskaElements.voidElement);
                        voidPosition = (int) dataStream.Position;
                    }
                    catch (Exception e) { }
                    

                    lsMkvFiles.Add(new MkvFile(filePath, trackList, voidPosition, tracksPosition));
                }
            }
            return lsMkvFiles;
        }

        private static List<byte> lsBytes;
        private static int offset;
        public static void WriteMkvFile(string filePath, List<Track> trackList, int voidPosition, int tracksPosition)
        {
            bool needToRewriteFile = voidPosition == 0;
            using (var memoryStream = new MemoryStream())
            using (var dataStream = File.Open(filePath, FileMode.Open))
            {
                if (needToRewriteFile)
                {
                    // Copy file contents to memory
                    dataStream.CopyTo(memoryStream);
                    lsBytes = new List<byte>(memoryStream.ToArray());
                    offset = 0;
                    
                    ApplyFlags(trackList);
                    
                    //lsBytes[trackList[0].trackLengthByteNumber - 2] += Convert.ToByte(offset);

                    // Write modiefied changes to file
                    dataStream.Seek(0, SeekOrigin.Begin);
                    dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
                }
                else
                {
                    int beginPosition = tracksPosition - 1; // Position of length of Tracks element
                    int endPosition = voidPosition + 100;
                    byte[] bytes = new byte[endPosition - beginPosition];

                    dataStream.Seek(beginPosition, SeekOrigin.Begin);
                    dataStream.Read(bytes, 0, bytes.Length);
                    lsBytes = new List<byte>(bytes);

                    offset = 0 - beginPosition;

                    ApplyFlags(trackList);

                    // Change length of Tracks element at 0x1654AE6B
                    lsBytes[0] += Convert.ToByte(beginPosition + offset);

                    // (change padding) Change void length after Tracks element and remove those bytes
                    lsBytes[offset + voidPosition - 1] -= Convert.ToByte(beginPosition + offset);
                    if (lsBytes.GetRange(offset + voidPosition, beginPosition + offset).Any(b => b != 0))
                    {
                        throw new Exception("Void is not long enough");
                    }
                    lsBytes.RemoveRange(offset + voidPosition, beginPosition + offset);

                    // Write modiefied changes to file
                    dataStream.Seek(beginPosition, SeekOrigin.Begin);
                    dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
                }
            }
        }

        private static void ApplyFlags(List<Track> trackList)
        {
            trackList
                .Where(x => x.type == TrackTypeEnum.audio || x.type == TrackTypeEnum.subtitle)
                .Where(x => x is not TrackDisable) // Maybe isn't needed?
                .ToList()
                .ForEach((Track t) =>
                {
                    byte defaultFlag = (byte) (t.flagDefault == true ? 0x1 : 0x0);
                    if (t.flagDefaultByteNumber != 0)
                    {
                        lsBytes[offset + t.flagDefaultByteNumber] = defaultFlag;
                    }
                    else if (t.flagTypebytenumber != 0)
                    {
                        // Change length of TrackEntry element 0xAE
                        lsBytes[offset + t.trackLengthByteNumber] += 0x3;

                        lsBytes.InsertRange(offset + t.flagTypebytenumber + 1,
                            new byte[] {0x88, 0x81, defaultFlag});
                        offset += 3;
                    }

                    if (t.flagForcedByteNumber != 0)
                    {
                        lsBytes[offset + t.flagForcedByteNumber] = 0x0;
                    }
                });
        }
    }
}
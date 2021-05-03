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
                    reader.LocateElement(MatroskaElements.voidElement);
                    int beginPosition = (int) reader.ElementPosition;
                    reader.LeaveContainer();
                    
                    reader.ReadNext();
                    int beginHeaderPosition = (int) reader.ElementPosition;
                    
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

                    reader.LocateElement(MatroskaElements.attachments);
                    int endPosition = (int) reader.ElementPosition - 1;

                    lsMkvFiles.Add(new MkvFile(filePath, trackList, beginPosition, endPosition, tracksPosition, beginHeaderPosition));
                }
            }
            return lsMkvFiles;
        }

        public static void WriteMkvFile(string filePath, List<Track> trackList, int beginPosition, int endPosition, int tracksPosition, int beginHeaderPosition = 0)
        {
            using (var dataStream = File.Open(filePath, FileMode.Open))
            {
                byte[] bytes = new byte[endPosition - beginPosition];
                
                dataStream.Seek(beginPosition, SeekOrigin.Begin);
                dataStream.Read(bytes, 0, bytes.Length);
                List<byte> lsBytes = new List<byte>(bytes);

                int offset = 0;

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

                // Change length of Tracks element at 0x1654AE6B
                lsBytes[tracksPosition - beginPosition] += Convert.ToByte(offset);

                lsBytes.RemoveRange(beginHeaderPosition - beginPosition - offset, offset);


                // Write modiefied changes to file
                dataStream.Seek(beginPosition, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
        }
    }
}
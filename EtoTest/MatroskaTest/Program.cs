using System;
using System.Collections.Generic;
using System.IO;

namespace MatroskaTest
{
    public class Program
    {
        const string GoodMkvPath = @"etotest\[DB]5-toubun no Hanayome_-_01_(Dual Audio_10bit_BD1080p_x265).mkv";
        const string CopyFilePath = @"etotest\TestFile.mkv";

        static List<MkvFile> lsMkvFiles;
        public static void Main()
        {
            // Copy file
            File.Copy(GoodMkvPath, CopyFilePath, true);

            // Read mkv and change default track
            ReadMkvFiles();
            int voidPosition = lsMkvFiles[0].voidPosition;
            List<Track> trackList = lsMkvFiles[0].tracks;
            trackList[2].flagDefault = false;
            trackList[3].flagDefault = true;
            trackList[8].flagDefault = true;
            // Write to mkv
            MatroskaLib.WriteMkvFile(CopyFilePath, trackList, voidPosition);
            // Read mkv again
            ReadMkvFiles();
        }

        private static void ReadMkvFiles()
        {
            Console.WriteLine(Environment.NewLine + "======");

            { 
                lsMkvFiles = MatroskaLib.ReadMkvFiles(new[] { CopyFilePath });
                
                List<Track> trackList = lsMkvFiles[0].tracks;
                int num = 0;
                trackList.ForEach((x) => Console.WriteLine(x));
            }
          
        }
    }
}

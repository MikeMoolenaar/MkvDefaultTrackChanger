using System;
using System.Collections.Generic;
using System.IO;

namespace MatroskaTest
{
    public class Program
    {
        const string GoodMkvPath = @"etotest\Kono Subarashii Sekai ni Shukufuku wo! Kurenai Densetsu [BDRip][1080p][HEVC10][AAC2.0].mkv";
        const string CopyFilePath = @"etotest\TestFile.mkv";

        static List<MkvFile> lsMkvFiles;
        public static void Main()
        {
            // Copy file
            File.Copy(GoodMkvPath, CopyFilePath, true);

            // Read mkv and change default track
            ReadMkvFiles();
            /*
            int voidPosition = lsMkvFiles[0].beginPosition;
            int tracksPosition = lsMkvFiles[0].tracksPosition;
            List<Track> trackList = lsMkvFiles[0].tracks;
            // Write to mkv
            MatroskaLib.WriteMkvFile(CopyFilePath, trackList, voidPosition, tracksPosition);
            // Read mkv again
            ReadMkvFiles();*/
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

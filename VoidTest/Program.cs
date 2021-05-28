﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatroskaTest;
using NEbml.Core;
using NEbml.Matroska;

namespace VoidTest
{
    class Program
    {
        static readonly string path = @"E:\Anime\";
        static void Main(string[] args)
        {
            Console.WriteLine("Processing...");
            string[] mkvFiles = Directory.GetFiles(path, "*.mkv", SearchOption.AllDirectories);
            var mainStringBuilder = new StringBuilder();

            int x = 0;
            Parallel.For(0, mkvFiles.Length, new ParallelOptions(){MaxDegreeOfParallelism = 4}, i => {
                var mkvFile = mkvFiles[i];
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Path.GetFileName(mkvFile));

                try
                {
                    var lsFiles = MatroskaLib.ReadMkvFiles(new[] {mkvFile});
                    stringBuilder.AppendLine("b" + lsFiles[0].tracks.Count);
                }
                catch (Exception e)
                {
                    stringBuilder.AppendLine(e.GetType().ToString());
                }
                stringBuilder.AppendLine();
                
                mainStringBuilder.Append(stringBuilder.ToString());
                Console.WriteLine($"{x++}/{mkvFiles.Length} {Path.GetFileName(mkvFile)}");
            });
            
            File.WriteAllText("Output2.txt", mainStringBuilder.ToString());
            Console.WriteLine("Done!");
        }
    }
}
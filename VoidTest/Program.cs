using System;
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
        static readonly string path = @"E:\Anime\- Torrents";
        static void Main(string[] args)
        {
            Console.WriteLine("Processing...");
            string[] mkvFiles = Directory.GetFiles(path, "*.mkv", SearchOption.AllDirectories);
            var mainStringBuilder = new StringBuilder();

            int x = 0;
            Parallel.For(0, mkvFiles.Length, new ParallelOptions(){MaxDegreeOfParallelism = 8}, i => {
                var mkvFile = mkvFiles[i];
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Path.GetFileName(mkvFile));

                try
                {
                    using (var dataStream = File.Open(mkvFile, FileMode.Open))
                    {
                        string voidElements = "b";
                        var reader = new EbmlReader(dataStream);

                        try
                        {
                            reader.LocateElement(MatroskaElements.segment);
                            reader.LocateElement(MatroskaElements.voidElement);
                            voidElements += "__1__" + dataStream.Position;
                        }
                        catch (Exception e) { }

                        try
                        {
                            reader.LeaveContainer();
                            reader.LocateElement(MatroskaElements.voidElement);

                            reader.LeaveContainer();
                            reader.LocateElement(MatroskaElements.clusterElement);
                            voidElements += "__2__";
                        }
                        catch (Exception e) { }
                    
                    
                        stringBuilder.AppendLine(voidElements);
                    }
                }
                catch (Exception e)
                {
                    stringBuilder.AppendLine(e.GetType().ToString());
                }
                stringBuilder.AppendLine();
                
                mainStringBuilder.Append(stringBuilder.ToString());
                Console.WriteLine($"{x++}/{mkvFiles.Length} {Path.GetFileName(mkvFile)}");
            });
            
            File.WriteAllText("Output.txt", mainStringBuilder.ToString());
            Console.WriteLine("Done!");
        }
    }
}
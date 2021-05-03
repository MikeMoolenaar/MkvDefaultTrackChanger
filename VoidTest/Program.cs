using System;
using System.IO;
using System.Text;
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
            Console.WriteLine("Hello World!");
            string[] mkvFiles = Directory.GetFiles(path, "*.mkv", SearchOption.AllDirectories);
            var stringBuilder = new StringBuilder();
            
            foreach (var mkvFile in mkvFiles)
            {
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
                            voidElements += "__1__";
                        }
                        catch (Exception e) { }

                        try
                        {
                            reader.LeaveContainer();
                            reader.LocateElement(MatroskaElements.voidElement);
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
                
            }
            
            File.WriteAllText("Output.txt", stringBuilder.ToString());
        }
    }
}
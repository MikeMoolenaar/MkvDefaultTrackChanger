using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MatroskaLib;

// Program to crawl through all MKV files and get the track count. 
// Crawls through specified folder and its subfolders.
namespace MkvReadCrawler;

static class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MkvReadCrawler <path-to-mkv-file-or-folder>");
            Console.WriteLine("\nTo test MediaInfo on a single file:");
            Console.WriteLine("  MkvReadCrawler \"C:\\path\\to\\file.mkv\"");
            return;
        }

        string path = args[0];

        if (File.Exists(path) && path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Testing single file with MediaInfo...");
            MediaInfoTest.TestMediaInfo(path);
            return;
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Path not found: {path}");
            return;
        }

        Console.WriteLine("Processing folder...");
        string[] mkvFiles = Directory.GetFiles(path, "*.mkv", SearchOption.AllDirectories);
        var mainStringBuilder = new StringBuilder();

        int x = 0;
        Parallel.For(0, mkvFiles.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
        {
            var mkvFile = mkvFiles[i];
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Path.GetFileName(mkvFile));

            try
            {
                var lsFiles = MatroskaReader.ReadMkvFiles([mkvFile]);
                MatroskaWriter.WriteMkvFile(lsFiles[0], dryRun:true);
                stringBuilder.AppendLine("Track count:" + lsFiles[0].tracks.Count);
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine(e.ToString());
            }
            stringBuilder.AppendLine();

            mainStringBuilder.Append(stringBuilder.ToString());
            Console.WriteLine($"{x++}/{mkvFiles.Length} {Path.GetFileName(mkvFile)}");
        });

        File.WriteAllText("output.txt", mainStringBuilder.ToString());
        Console.WriteLine("Done! Output written to output.txt");
    }
}

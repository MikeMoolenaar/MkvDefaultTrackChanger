using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MatroskaLib.Test.Helpers;

public static class MkvValidator
{
    private const string OutputRemoveRegex = @"(^At least one output file must be specified)|(^\[(.*)\] )";
    public static void Validate(string filePath)
    {
        // Validate with ffmpeg
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = "ffmpeg";
        p.StartInfo.Arguments = $"-i \"{filePath}\" -v error";
        p.Start();
        string output = p.StandardError.ReadToEnd();
        p.Close();
        output = Regex.Replace(output, OutputRemoveRegex, "", RegexOptions.Multiline).Trim();
        if (output.Length > 0)
        {
            throw new Exception("ffmpeg's mkv validation produced errors:" + Environment.NewLine + output);
        }
        
        // Validate with mkvalidator
        p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
            "mkvalidator.exe" : 
            "mkvalidator";
        p.StartInfo.Arguments = $"\"{filePath}\"";
        p.Start();
        output = p.StandardError.ReadToEnd();
        p.Close();

        if (!output.Contains("the file appears to be valid"))
        {
            output = output.Replace(".", "").Trim();
            string errors = string.Join("\n", output.Split("\r\n").Where(x => x.Contains("ERR"))).Trim();
            throw new Exception(errors + "\r\n" + output);
        }
    }
}

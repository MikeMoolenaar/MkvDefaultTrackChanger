using System;
using System.Diagnostics;

namespace Matroska.Test.Helpers
{
    public class MkvValidator
    {
        public static void Validate(string filePath)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "./etotest/mkvalidator.exe";
            p.StartInfo.Arguments = filePath;
            p.Start();
            string output = p.StandardError.ReadToEnd();
            p.Close();

            if (!output.Contains("the file appears to be valid"))
            {
                throw new Exception(output);
            }
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace ByteWritingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Write base file
            using (var dataStream = File.Open("SomeFile", FileMode.Create))
            {
                dataStream.Write(new byte[] { 0x1, 0x8, 0x9 }, 0, 3);
            }

            // Method #1: Insert 0x40 in second position 
            // NOTE: Because lists have a limit, a maximum of Int32.MaxValue bytes
            //  can be used, which is about 2GB
            using (var dataStream = File.Open("SomeFile", FileMode.Open))
            {
                List<byte> lsBytes = new List<byte>();

                var bytes = new byte[3];
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Read(bytes, 0, bytes.Length);

                lsBytes.AddRange(bytes);
                lsBytes.Insert(1, 0x40);

                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
            // Result is 0x1 0x40 0x8 0x9

            // Method #2
            using (var dataStream = File.Open("SomeFile", FileMode.Open))
            using (var memoryStream = new MemoryStream())
            {
                dataStream.CopyTo(memoryStream);
                List<byte> lsBytes = new List<byte>(memoryStream.ToArray());

                lsBytes.Insert(1, 0x41);

                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(lsBytes.ToArray(), 0, lsBytes.Count);
            }
            // Result is 0x1 0x41 0x40 0x8 0x9
        }
    }
}

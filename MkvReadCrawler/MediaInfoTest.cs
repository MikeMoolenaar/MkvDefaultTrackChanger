using System;
using MediaInfo;
using Microsoft.Extensions.Logging.Abstractions;

namespace MkvReadCrawler;

public static class MediaInfoTest
{
    public static void TestMediaInfo(string filePath)
    {
        Console.WriteLine($"\n=== Testing MediaInfo on: {filePath} ===\n");
        
        try
        {
            var mediaInfo = new MediaInfoWrapper(filePath, NullLogger.Instance);
            
            if (!mediaInfo.Success)
            {
                Console.WriteLine("MediaInfo failed to open file!");
                return;
            }
            
            Console.WriteLine($"File opened successfully!");
            Console.WriteLine($"Format: {mediaInfo.Format}");
            Console.WriteLine($"Duration: {mediaInfo.Duration}");
            Console.WriteLine($"\nVideo Streams: {mediaInfo.VideoStreams?.Count ?? 0}");
            Console.WriteLine($"Audio Streams: {mediaInfo.AudioStreams?.Count ?? 0}");
            Console.WriteLine($"Subtitle Streams: {mediaInfo.Subtitles?.Count ?? 0}");
            
            if (mediaInfo.AudioStreams != null)
            {
                Console.WriteLine("\n--- Audio Streams ---");
                for (int i = 0; i < mediaInfo.AudioStreams.Count; i++)
                {
                    var audio = mediaInfo.AudioStreams[i];
                    Console.WriteLine($"\nAudio Stream {i}:");
                    Console.WriteLine($"  ID: {audio.Id}");
                    Console.WriteLine($"  StreamNumber: {audio.StreamNumber}");
                    Console.WriteLine($"  StreamPosition: {audio.StreamPosition}");
                    Console.WriteLine($"  Format: '{audio.Format}'");
                    Console.WriteLine($"  CodecDescription: '{audio.CodecDescription}'");
                    Console.WriteLine($"  CodecFriendly: '{audio.CodecFriendly}'");
                    Console.WriteLine($"  CodecName: '{audio.CodecName}'");
                    Console.WriteLine($"  Language: '{audio.Language}'");
                    Console.WriteLine($"  Name: '{audio.Name}'");
                    Console.WriteLine($"  Channels: {audio.Channel}");
                    Console.WriteLine($"  AudioChannelsFriendly: '{audio.AudioChannelsFriendly}'");
                    Console.WriteLine($"  Bitrate: {audio.Bitrate}");
                    Console.WriteLine($"  BitrateMode: {audio.BitrateMode}");
                    Console.WriteLine($"  SamplingRate: {audio.SamplingRate}");
                    
                    // Check if Tags has any format settings info
                    if (audio.Tags != null)
                    {
                        Console.WriteLine($"  Tags.Title: '{audio.Tags.Title}'");
                        Console.WriteLine($"  Tags.Comment: '{audio.Tags.Comment}'");
                    }
                }
            }
            
            if (mediaInfo.Subtitles != null)
            {
                Console.WriteLine("\n--- Subtitle Streams ---");
                for (int i = 0; i < mediaInfo.Subtitles.Count; i++)
                {
                    var subtitle = mediaInfo.Subtitles[i];
                    Console.WriteLine($"\nSubtitle Stream {i}:");
                    Console.WriteLine($"  ID: {subtitle.Id}");
                    Console.WriteLine($"  StreamNumber: {subtitle.StreamNumber}");
                    Console.WriteLine($"  Format: '{subtitle.Format}'");
                    Console.WriteLine($"  Language: '{subtitle.Language}'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

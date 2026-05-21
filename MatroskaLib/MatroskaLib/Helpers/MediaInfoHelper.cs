using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MediaInfo;
using MediaInfo.Model;
using MatroskaLib.Types;
using Microsoft.Extensions.Logging.Abstractions;

namespace MatroskaLib.Helpers;

public static class MediaInfoHelper
{
    public static void GetAllTrackFormats(Stream stream, IEnumerable<Track> tracks)
    {
        
        try
        {
            if (!stream.CanSeek)
            {
                Debug.WriteLine($"Stream is not seekable, cannot use MediaInfo");
                return;
            }
            
            long originalPosition = stream.Position;
            stream.Position = 0;
            
            var mediaInfo = new MediaInfoWrapper(stream, NullLogger.Instance);
            
            stream.Position = originalPosition;
            
            if (!mediaInfo.Success)
            {
                Debug.WriteLine($"MediaInfo failed to read from stream");
                return;
            }
            
            Debug.WriteLine($"MediaInfo found {mediaInfo.AudioStreams?.Count ?? 0} audio streams, {mediaInfo.Subtitles?.Count ?? 0} subtitle streams");
            
            foreach (var track in tracks)
            {           
                if (track.type == TrackTypeEnum.audio)
                {
                    if (mediaInfo.AudioStreams != null && mediaInfo.AudioStreams.Any())
                    {
                        var audioStream = mediaInfo.AudioStreams.FirstOrDefault(a => a.Id == (int)track.number);
                        
                        if (audioStream != null)
                        {
                            Debug.WriteLine($"Track #{track.number} (audio) - Format: '{audioStream.Format}', CodecDesc: '{audioStream.CodecDescription}', Bitrate: {audioStream.Bitrate}");
                            var format = string.Empty;

                            if (!string.IsNullOrEmpty(audioStream.CodecDescription))
                                format = audioStream.CodecDescription;
                            else if (!string.IsNullOrEmpty(audioStream.CodecFriendly))
                                format = audioStream.CodecFriendly;
                            else if (!string.IsNullOrEmpty(audioStream.Format))
                                format = audioStream.Format;

                            track.detectedFormat = format ?? string.Empty;
                            track.bitrate = audioStream.Bitrate;
                        }
                        else
                        {
                            Debug.WriteLine($"Track #{track.number} (audio) - No matching MediaInfo stream found");
                        }
                    }
                }
                else if (track.type == TrackTypeEnum.subtitle)
                {
                    var subtitleStream = mediaInfo.Subtitles?.FirstOrDefault(s => s.Id == (int)track.number);
                        
                    if (subtitleStream != null)
                    {
                        Debug.WriteLine($"Track #{track.number} (subtitle) - Format: '{subtitleStream.Format}'");
                        track.detectedFormat = subtitleStream.Format ?? string.Empty;
                        track.bitrate = 0; // Subtitles typically don't have a bitrate, so we set it to 0
                    }
                    else
                    {
                        Debug.WriteLine($"Track #{track.number} (subtitle) - No matching MediaInfo stream found");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MediaInfoHelper exception: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

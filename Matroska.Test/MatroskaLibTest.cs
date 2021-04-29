using System;
using System.Collections.Generic;
using System.IO;
using MatroskaTest;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

namespace Matroska.Test
{
    public class MatroskaLibTest
    {
        private const string testFilePath = "etotest/TestFile.mkv";
        private readonly ITestOutputHelper _testOutputHelper;

        public MatroskaLibTest(ITestOutputHelper testOutputHelper)
        {
            //File.Copy(file, testFilePath, true);
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("etotest/[HorribleSubs] 3D Kanojo Real Girl 2 - 1 [1080p].mkv")]
        public void ReadMkvFilesTest(string file)
        {
            string[] filePaths = {file};

            List<MkvFile> lsMkvFiles = MatroskaLib.ReadMkvFiles(filePaths);
            List<Track> tracks = lsMkvFiles[0].tracks;

            Assert.Single(lsMkvFiles);
            Assert.Equal(5074, lsMkvFiles[0].voidPosition);
            Assert.Equal(3, tracks.Count);
            tracks[0].Should().BeEquivalentTo(new { flagDefault = false, flagForced = false, language = "und" });
            tracks[1].Should().BeEquivalentTo(new { flagDefault = false, flagForced = false, language = "jpn" });
            tracks[2].Should().BeEquivalentTo(new { flagDefault = false, flagForced = true, language = "eng" });
        }
    }
}
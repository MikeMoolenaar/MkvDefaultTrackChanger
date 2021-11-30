using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using MatroskaLib.Test.Helpers;

namespace MatroskaLib.Test
{
    /* MkvToolNix
     *   Both voids, default track elements not present
     * Handbrake
     *   Only first void with checksum elements
     * MkvProEdit
     *   Only second void and may need to change length of that void
     */
    public class MatroskaLibTest
    {
        private const string testFilePath = "mkv files/TestFile.mkv";
        private readonly ITestOutputHelper _testOutputHelper;

        public MatroskaLibTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("mkv files/TestFile1_MkvToolNix.mkv")]
        public void ReadTestFile1(string file)
        {
            string[] filePaths = {file};
            
            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(filePaths);
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsMkvFiles.Should().HaveCount(1);
            lsMkvFiles[0].voidPosition.Should().Be(119);
            lsTracks.Should().HaveCount(3);
            lsTracks[0].Should().BeEquivalentTo(new {number = 1, flagDefault = false, flagForced = true, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[1].Should().BeEquivalentTo(new {number = 2, flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
            lsTracks[2].Should().BeEquivalentTo(new {number = 3, flagDefault = false, flagForced = false, language = "jpn", type = TrackTypeEnum.audio});
        }
        
        [Theory]
        [InlineData("mkv files/TestFile1_MkvToolNix.mkv")]
        public void WriteTestFile1(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[0].flagDefault = false;
            lsMkvFiles[0].tracks[2].flagDefault = false;

            MatroskaIO.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsTracks.Should().HaveCount(3);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "jpn", type = TrackTypeEnum.audio});
            MkvValidator.Validate(testFilePath);
        }
        
        [Theory]
        [InlineData("mkv files/TestFile2_MkvToolNix.mkv")]
        public void ReadTestFile2(string file)
        {
            string[] filePaths = {file};

            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(filePaths);
            List<Track> lsTracks = lsMkvFiles[0].tracks;
            
            lsMkvFiles.Should().HaveCount(1);
            lsMkvFiles[0].voidPosition.Should().Be(119);
            lsTracks.Should().HaveCount(5);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "fre", name = "French commentary", type = TrackTypeEnum.audio});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "eng", name = "", type = TrackTypeEnum.audio});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = true, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[3].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "jpn", name = "日本語", type = TrackTypeEnum.subtitle});
            lsTracks[4].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
        }

        [Theory]
        [InlineData("mkv files/TestFile2_MkvToolNix.mkv")] 
        public void WriteTestFile2(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[1].flagDefault = true;
            lsMkvFiles[0].tracks[3].flagDefault = true;

            MatroskaIO.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsTracks.Should().HaveCount(5);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "fre", name = "French commentary", type = TrackTypeEnum.audio});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "eng", name = "", type = TrackTypeEnum.audio});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[3].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "jpn", name = "日本語", type = TrackTypeEnum.subtitle});
            lsTracks[4].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
            MkvValidator.Validate(testFilePath);
        }

        [Theory]
        [InlineData("mkv files/TestFile3_HandBrake.mkv")]
        public void ReadTestFile3(string file)
        {
            string[] filePaths = {file};

            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(filePaths);
            List<Track> lsTracks = lsMkvFiles[0].tracks;
            
            lsMkvFiles.Should().HaveCount(1);
            lsMkvFiles[0].voidPosition.Should().Be(123);
            lsTracks.Should().HaveCount(4);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", name = "", type = TrackTypeEnum.video});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", name = "Stereo", type = TrackTypeEnum.audio});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = true, language = "eng", name = "", type = TrackTypeEnum.subtitle});
            lsTracks[3].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "jpn", name = "", type = TrackTypeEnum.subtitle});
        }
        
        [Theory]
        [InlineData("mkv files/TestFile3_HandBrake.mkv")] 
        public void WriteTestFile3(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[1].flagDefault = true;
            lsMkvFiles[0].tracks[3].flagDefault = true;

            MatroskaIO.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsTracks.Should().HaveCount(4);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", name = "", type = TrackTypeEnum.video});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "und", name = "Stereo", type = TrackTypeEnum.audio});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "eng", name = "", type = TrackTypeEnum.subtitle});
            lsTracks[3].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "jpn", name = "", type = TrackTypeEnum.subtitle});
            MkvValidator.Validate(testFilePath);
        }
        
        [Theory]
        [InlineData("mkv files/TestFile4_MkvProEdit.mkv")]
        [InlineData("mkv files/TestFile5_MkvProEdit.mkv")]
        public void ReadTestFile4(string file)
        {
            string[] filePaths = {file};

            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(filePaths);
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsMkvFiles.Should().HaveCount(1);
            lsTracks.Should().HaveCount(3);
            lsTracks[0].Should().BeEquivalentTo(new {number = 1, flagDefault = false, flagForced = false, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[1].Should().BeEquivalentTo(new {number = 2, flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
            lsTracks[2].Should().BeEquivalentTo(new {number = 3, flagDefault = false, flagForced = false, language = "jpn", type = TrackTypeEnum.audio});
        }
        
        [Theory]
        [InlineData("mkv files/TestFile4_MkvProEdit.mkv")]
        [InlineData("mkv files/TestFile5_MkvProEdit.mkv")]
        public void WriteTestFile4(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[0].flagDefault = false;
            lsMkvFiles[0].tracks[2].flagDefault = false;

            MatroskaIO.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            lsMkvFiles = MatroskaIO.ReadMkvFiles(new[] {testFilePath});
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            lsTracks.Should().HaveCount(3);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "eng", name = "English main", type = TrackTypeEnum.subtitle});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und", type = TrackTypeEnum.video});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "jpn", type = TrackTypeEnum.audio});
            MkvValidator.Validate(testFilePath);
        }


    }
}
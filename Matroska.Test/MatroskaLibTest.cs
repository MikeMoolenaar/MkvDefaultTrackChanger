using System;
using System.Collections.Generic;
using System.IO;
using MatroskaTest;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Matroska.Test.Helpers;

namespace Matroska.Test
{
    public class MatroskaLibTest
    {
        private const string testFilePath = "etotest/TestFile.mkv";
        private readonly ITestOutputHelper _testOutputHelper;

        public MatroskaLibTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("etotest/[HorribleSubs] 3D Kanojo Real Girl - 12 [1080p].mkv")]
        public void ReadMkvFilesTest(string file)
        {
            string[] filePaths = {file};

            List<MkvFile> lsMkvFiles = MatroskaLib.ReadMkvFiles(filePaths);
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            Assert.Single(lsMkvFiles);
            Assert.Equal(136, lsMkvFiles[0].voidPosition);
            Assert.Equal(3, lsTracks.Count);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und"});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "jpn"});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = false, flagForced = true, language = "eng"});
        }

        [Theory]
        [InlineData("etotest/[HorribleSubs] 3D Kanojo Real Girl - 12 [1080p].mkv")]
        public void WriteMkvFileTest(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaLib.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[1].flagDefault = true;
            lsMkvFiles[0].tracks[2].flagDefault = true;

            MatroskaLib.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            lsMkvFiles = MatroskaLib.ReadMkvFiles(new[] {testFilePath});
            List<Track> lsTracks = lsMkvFiles[0].tracks;

            Assert.Equal(3, lsTracks.Count);
            lsTracks[0].Should().BeEquivalentTo(new {flagDefault = false, flagForced = false, language = "und"});
            lsTracks[1].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "jpn"});
            lsTracks[2].Should().BeEquivalentTo(new {flagDefault = true, flagForced = false, language = "eng"});
            MkvValidator.Validate(testFilePath);
        }

        [Theory]
        [InlineData("etotest/[ASW] Wonder Egg Priority - S01E05 [1080p HEVC].mkv")]
        public void ReadMkvFileWithoutVoidTest(string file)
        {
            File.Copy(file, testFilePath, true);
            List<MkvFile> lsMkvFiles = MatroskaLib.ReadMkvFiles(new[] {testFilePath});
            lsMkvFiles[0].tracks[2].flagDefault = false;
            MatroskaLib.WriteMkvFile(testFilePath, lsMkvFiles[0].seekList, lsMkvFiles[0].tracks, lsMkvFiles[0].seekHeadCheckSum, lsMkvFiles[0].tracksCheckSum, lsMkvFiles[0].voidPosition,
                lsMkvFiles[0].endPosition, lsMkvFiles[0].tracksPosition, lsMkvFiles[0].beginHeaderPosition);
            MkvValidator.Validate(testFilePath);
        }
        
        [Theory]
        [MemberData(nameof(Data))]

        public void TestChangeLength(List<byte> inputData, List<byte> expectedData, int position, int newAddition)
        {
            MatroskaLib.ChangeLength(inputData, position, 0xAE, newAddition);
            Assert.Equal(inputData, expectedData);
        }
        
        public static IEnumerable<object[]> Data() {
            yield return new object[]
            {
                new List<byte>{ 0x6B, 0x2D, 0xAE, 0xBB, 0xD7, 0x81, 0x02 }, 
                new List<byte>{ 0x6B, 0x2D, 0xAE, 0xBE, 0xD7, 0x81, 0x02 },
                4,
                3
            };
            yield return new object[]
            {
                new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x83, 0xD7, 0x81, 0x03 }, 
                new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x87, 0xD7, 0x81, 0x03 }, 
                5,
                4
            };
            yield return new object[]
            {
                new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x83, 0xD7, 0x81, 0x03 }, 
                new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x87, 0xD7, 0x81, 0x03 }, 
                5,
                4
            };
            yield return new object[]
            {
                new List<byte>{ 0x00, 0x00, 0xAE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x3A, 0xD7, 81 }, 
                new List<byte>{ 0x00, 0x00, 0xAE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x3D, 0xD7, 81 },
                11,
                3
            };
        }
    }
}
using System.Collections.Generic;
using Xunit;

namespace MatroskaLib.Test
{
    public class ByteHelperTest
    {
        public static TheoryData<ulong, List<byte>> TestData1 = new () {
            { 2UL,  new () { 0x2 } },
            { 909UL,  new () { 0x3, 0x8D } },
            { 1_800_70UL,  new () { 0x2, 0xBF, 0x66 } },
        };
        [Theory, MemberData(nameof(TestData1))]
        public void ToBytesTest(ulong value, List<byte> lsBytesExpected)
        {
            List<byte> lsResult = ByteHelper.ToBytes(value);
            
            Assert.Equal(lsBytesExpected, lsResult);
        }

        public static TheoryData<List<byte>, List<byte>> TestData2 = new () {
            { new() {0x0, 0x0, 0x0, 0x96},  new () { 0x96 } },
            { new() {0x0, 0x0, 0x5, 0x0, 0x9}, new () { 0x5, 0x0, 0x9 } },
            { new() {0x9}, new () { 0x9 } },
            { new() {}, new () {  } }
        };
        [Theory, MemberData(nameof(TestData2))]
        public void RemoveLeftZeroesTest(List<byte> lsBytes, List<byte> lsBytesExpected)
        {
            ByteHelper.RemoveLeftZeroes(lsBytes);

            Assert.Equal(lsBytes, lsBytesExpected);
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
        [Theory, MemberData(nameof(Data))]

        public void TestChangeLength(List<byte> inputData, List<byte> expectedData, int position, int newAddition)
        {
            ByteHelper.ChangeLength(inputData, position, 0xAE, newAddition);
            
            Assert.Equal(inputData, expectedData);
        }
        
        
    }
}
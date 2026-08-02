using System.Collections.Generic;
using Xunit;

namespace MatroskaLib.Test;

public class ByteHelperTest
{
    public static TheoryData<ulong, List<byte>> ToBytesData = new() {
        { 2UL, [0x2] },
        { 909UL, [0x3, 0x8D] },
        { 1_800_70UL, [0x2, 0xBF, 0x66] },
    };
    [Theory, MemberData(nameof(ToBytesData))]
    public void ToBytesTest(ulong value, List<byte> lsBytesExpected)
    {
        List<byte> lsResult = ByteHelper.ToBytes(value);

        Assert.Equal(lsBytesExpected, lsResult);
    }

    public static TheoryData<List<byte>, List<byte>> RemoveLeftZeroesData = new() {
        { [0x0, 0x0, 0x0, 0x96], [0x96] },
        { [0x0, 0x0, 0x5, 0x0, 0x9], [0x5, 0x0, 0x9] },
        { [0x9], [0x9] },
        { [], [] }
    };
    [Theory, MemberData(nameof(RemoveLeftZeroesData))]
    public void RemoveLeftZeroesTest(List<byte> lsBytes, List<byte> lsBytesExpected)
    {
        ByteHelper.RemoveLeftZeroes(lsBytes);

        Assert.Equal(lsBytes, lsBytesExpected);
    }

    public static TheoryData<List<byte>, List<byte>, int> AddLeftZeroesData = new() {
        { [0x96], [0x0, 0x0, 0x96], 3 },
        { [0x0, 0x0, 0x96], [0x0, 0x0, 0x0, 0x96], 4 },
        { [0x9], [0x9], 1 },
        { [], [], 0 }
    };
    [Theory, MemberData(nameof(AddLeftZeroesData))]
    public void AddLeftZeroesTest(List<byte> lsBytes, List<byte> lsBytesExpected, int length)
    {
        ByteHelper.AddLeftZeroes(lsBytes, length);

        Assert.Equal(lsBytes, lsBytesExpected);
    }

    public static IEnumerable<object[]> ChangeLengthData()
    {
        yield return
        [
            new List<byte>{ 0x6B, 0x2D, 0xAE, 0xBB, 0xD7, 0x81, 0x02 },
            new List<byte>{ 0x6B, 0x2D, 0xAE, 0xBE, 0xD7, 0x81, 0x02 },
            4,
            3
        ];
        yield return
        [
            new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x83, 0xD7, 0x81, 0x03 },
            new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x87, 0xD7, 0x81, 0x03 },
            5,
            4
        ];
        yield return
        [
            new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x83, 0xD7, 0x81, 0x03 },
            new List<byte>{ 0x81, 0x02, 0xAE, 0x42, 0x87, 0xD7, 0x81, 0x03 },
            5,
            4
        ];
        yield return
        [
            new List<byte>{ 0x00, 0x00, 0xAE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x3A, 0xD7, 81 },
            new List<byte>{ 0x00, 0x00, 0xAE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x3D, 0xD7, 81 },
            11,
            3
        ];
    }
    [Theory, MemberData(nameof(ChangeLengthData))]
    public void TestChangeLength(List<byte> inputData, List<byte> expectedData, int position, int newAddition)
    {
        ByteHelper.ChangeLength(inputData, position, 0xAE, newAddition);

        Assert.Equal(inputData, expectedData);
    }
}

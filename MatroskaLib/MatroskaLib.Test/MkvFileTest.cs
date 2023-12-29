using FluentAssertions;
using MatroskaLib.Types;
using Xunit;

namespace MatroskaLib.Test;

public class MkvFileTest
{
    [Fact]
    public void TestToString()
    {
        var mkvFile = new MkvFile()
        {
            filePath = "/home/some-path",
            tracks = [new Track(null!) { type = TrackTypeEnum.subtitle, flagForced = true }],
            endPosition = 20,
            beginHeaderPosition = 30,
            seekHeadCheckSum = null,
            seekList = [],
            tracksCheckSum = 50,
            tracksPosition = 30,
            voidPosition = 40
        };

        string json = mkvFile.ToString();

        json.Should().Be("""
                         {
                           "filePath": "",
                           "tracks": [
                             {
                               "trackLengthByteNumber": 0,
                               "number": 0,
                               "flagDefault": false,
                               "flagDefaultByteNumber": 0,
                               "flagForced": true,
                               "flagForcedByteNumber": 0,
                               "flagTypebytenumber": 0,
                               "type": "subtitle",
                               "name": "",
                               "language": "eng"
                             }
                           ],
                           "seekList": [],
                           "seekHeadCheckSum": null,
                           "tracksCheckSum": 50,
                           "voidPosition": 40,
                           "endPosition": 20,
                           "tracksPosition": 30,
                           "beginHeaderPosition": 30
                         }
                         """);
    }
}

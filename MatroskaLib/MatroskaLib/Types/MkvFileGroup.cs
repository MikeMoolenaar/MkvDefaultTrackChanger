using System.Collections.Generic;

namespace MatroskaLib.Types;

public class MkvFileGroup
{
    public List<MkvFile> Files { get; } = new();
    public MkvFile Reference => Files[0];

    public MkvFileGroup(MkvFile file) => Files.Add(file);
}

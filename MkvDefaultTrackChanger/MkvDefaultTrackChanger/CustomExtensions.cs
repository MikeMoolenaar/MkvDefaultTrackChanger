using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using MatroskaLib.Types;

namespace MkvDefaultTrackChanger;

public static class CustomExtensions
{
    public static IEnumerable<IListItem> ToEnoListItems(this IEnumerable<Track> ls)
    {
        return ls.Select(track => new ListItem
        {
            Key = track.number.ToString(),
            Text = track.ToUiString()
        });
    }
}

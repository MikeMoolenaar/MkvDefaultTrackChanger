using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using MatroskaLib;

namespace MkvDefaultSwitcher2
{
    public static class CustomExtensions
    {
        public static IEnumerable<IListItem> ToEnoListItems(this List<Track> ls)
        {
            return ls.Select((Track track) => new ListItem()
            {
                Key = track.number.ToString(),
                Text = track.ToUiString()
            });
        }
    }
}
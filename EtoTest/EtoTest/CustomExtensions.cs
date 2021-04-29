using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatroskaTest;

namespace EtoTest
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

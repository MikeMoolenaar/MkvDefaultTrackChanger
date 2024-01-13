using System;
using Eto.Forms;

namespace MkvDefaultTrackChanger.GtkSharp
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.GtkSharp.Platform();
            new Application(platform).Run(new MainForm());
        }
    }
}

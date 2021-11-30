using Eto.Forms;
using System;

namespace MkvDefaultTrackChanger.Mac
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Mac.Platform();
            new Application(platform).Run(new MainForm());
        }
    }
}
using System;
using Eto.Forms;

namespace MkvDefaultTrackChanger.Mac;

static class MainClass
{
    [STAThread]
    public static void Main(string[] args)
    {
        var platform = new Eto.Mac.Platform();
        new Application(platform).Run(new MainForm());
    }
}

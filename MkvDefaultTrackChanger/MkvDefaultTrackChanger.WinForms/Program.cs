using System;
using Eto.Forms;

namespace MkvDefaultTrackChanger.WinForms;

static class MainClass
{
    [STAThread]
    public static void Main(string[] args)
    {
        var platform = new Eto.WinForms.Platform();
        new Application(platform).Run(new MainForm());

    }
}

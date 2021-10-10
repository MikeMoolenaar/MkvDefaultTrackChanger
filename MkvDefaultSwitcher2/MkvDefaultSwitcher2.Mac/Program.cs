using Eto.Forms;
using System;

namespace MkvDefaultSwitcher2.Mac
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Mac.Platform();
            platform.Add<OpenFileDialog.IHandler>(() => new CustomFileDialogHandler());
            new Application(platform).Run(new MainForm());
        }
    }
}
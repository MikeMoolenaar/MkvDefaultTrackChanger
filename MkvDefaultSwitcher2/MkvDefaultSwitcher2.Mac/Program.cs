using Eto.Forms;
using System;
using Eto.Mac.Forms;

namespace MkvDefaultSwitcher2.Mac
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // TODO test if multiple file works on Mac
            var platform = new Eto.Mac.Platform();
            platform.Add<OpenFileDialog.IHandler>(() =>
            {
                var handler = new OpenFileDialogHandler();
                handler.MultiSelect = true;
                return handler;
            });
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
            
        }
    }
}

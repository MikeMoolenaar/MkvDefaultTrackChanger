using Eto.Forms;
using System;
using EtoTest.CustomControls;

namespace EtoTest.Mac
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // TODO test if Filepicker works on Mac
            var platform = new Eto.Mac.Platform();
            platform.Add<FilePicker.IHandler>(() => new CustomFileHandler());
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
            
        }
    }
}

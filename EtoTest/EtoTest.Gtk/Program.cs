using Eto.Forms;
using System;


namespace EtoTest.GtkSharp
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.GtkSharp.Platform();
            platform.Add<FilePicker.IHandler>(() => new FilePickerHandler());
            new Application(platform).Run(new MainForm());
        }
    }
}

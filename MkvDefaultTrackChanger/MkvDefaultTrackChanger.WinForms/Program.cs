using System;
using Eto.Forms;
using swf = System.Windows.Forms;

namespace MkvDefaultTrackChanger.WinForms;

static class MainClass
{
    [STAThread]
    public static void Main(string[] args)
    {
        var platform = new Eto.WinForms.Platform();
        Eto.Style.Add<DropDown>(null, control =>
        {
            var dropdown = control.ControlObject as swf.ComboBox;
            dropdown.DrawMode = swf.DrawMode.Normal;
        });
        
        new Application(platform).Run(new MainForm());

    }
}

using System;
using System.Runtime.InteropServices;
using Eto.Forms;
using swf = System.Windows.Forms;


//private const int ATTACH_PARENT_PROCESS = -1;

namespace MkvDefaultTrackChanger.WinForms;

static class MainClass
{
    [DllImport("kernel32.dll")]
    static extern bool AttachConsole(int dwProcessId);

    [STAThread]
    public static void Main(string[] args)
    {
        AttachConsole(-1);
        var platform = new Eto.WinForms.Platform();
        Eto.Style.Add<DropDown>(null, control =>
        {
            var dropdown = control.ControlObject as swf.ComboBox;
            dropdown.DrawMode = swf.DrawMode.Normal;
        });
        
        new Application(platform).Run(new MainForm(args));

    }
}

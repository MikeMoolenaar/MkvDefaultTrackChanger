using Eto.Forms;
using System;
using EtoTest.CustomControls;
using swf = System.Windows.Forms;

namespace EtoTest.WinForms
{

    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.WinForms.Platform();
            platform.Add<FilePicker.IHandler>(() => new CustomFileHandler());

            Eto.Style.Add<DropDown>(null, control => {
                var dropdown = control.ControlObject as swf.ComboBox;
                dropdown.DrawMode = swf.DrawMode.Normal;
            });
            Eto.Style.Add<ListBox>(null, control => {
                var listBox = control.ControlObject as swf.ListBox;
                listBox.SelectionMode = swf.SelectionMode.None;
            });

            new Application(platform).Run(new MainForm());
            
        }
    }
}

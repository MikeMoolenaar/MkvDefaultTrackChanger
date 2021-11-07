using Eto.Forms;
using Eto.Mac.Forms;

namespace MkvDefaultSwitcher.Mac
{
    using MonoMac.AppKit;
    using MonoMac.Foundation;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CustomFileDialogHandler : 
        MacFileDialog<NSOpenPanel, OpenFileDialog>,
        OpenFileDialog.IHandler
    {
        public CustomFileDialogHandler()
        {
            this.MultiSelect = true;
        }

        protected override NSOpenPanel CreateControl() => NSOpenPanel.OpenPanel;

        protected override bool DisposeControl => false;

        public override string FileName {
            get => String.Join("|", this.Filenames);
            set { }
        }

        public bool MultiSelect
        {
            get => this.Control.AllowsMultipleSelection;
            set => this.Control.AllowsMultipleSelection = value;
        }

        public IEnumerable<string> Filenames => ((IEnumerable<NSUrl>) this.Control.Urls).Select<NSUrl, string>((Func<NSUrl, string>) (a => a.Path));
    }
}
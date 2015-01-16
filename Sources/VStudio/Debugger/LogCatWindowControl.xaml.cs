using System.Windows.Controls;
using Dot42.Ide;
using Dot42.Ide.Debugger;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Interaction logic for LogCatWindowControl.xaml
    /// </summary>
    public partial class LogCatWindowControl : UserControl
    {
        public LogCatWindowControl(IIde ide)
        {
            InitializeComponent();
            controlHost.Child = new DeviceLogControl(ide);
        }
    }
}

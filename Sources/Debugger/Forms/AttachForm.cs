using Dot42.Shared.UI;

namespace Dot42.Debugger.Forms
{
    public partial class AttachForm : AppForm
    {
        public AttachForm()
        {
            InitializeComponent();
        }

        private void tbPid_ValueChanged(object sender, System.EventArgs e)
        {
            cmdOk.Enabled = tbPid.Value > 0;
        }

        public int Pid { get { return (int) tbPid.Value; } }
    }
}

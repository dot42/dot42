using System;
using System.Windows.Forms;

namespace Dot42.CryptoUI.Controls
{
    public partial class CreatingCertificateControl : UserControl
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public CreatingCertificateControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Log the given message.
        /// </summary>
        internal void Log(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), msg);
            }
            else
            {
                tbLog.AppendText(msg + Environment.NewLine);
            }
        }
    }
}

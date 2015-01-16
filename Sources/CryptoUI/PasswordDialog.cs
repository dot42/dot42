using System.Windows.Forms;

namespace Dot42.CryptoUI
{
    public partial class PasswordDialog : Form
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }

        public string Password { get { return tbPassword.Text; }}
    }
}

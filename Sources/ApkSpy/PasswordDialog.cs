using System;
using System.Windows.Forms;

namespace Dot42.ApkSpy
{
    public partial class PasswordDialog : Form
    {
        public PasswordDialog()
        {
            InitializeComponent();
            tbPassword_TextChanged(null, null);
        }

        public string Password { get { return tbPassword.Text; } }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            cmdOk.Enabled = (tbPassword.Text.Length > 0);
        }

        public static string Ask()
        {
            using (var dialog = new PasswordDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return null;
                return dialog.Password;
            }
        }
    }
}

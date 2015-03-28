using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Dot42.ApkSpy
{
    public partial class ConfigureBaksmaliDialog : Form
    {
        public ConfigureBaksmaliDialog()
        {
            InitializeComponent();
        }

        public string BaksmaliCommand { get { return tbCommand.Text; } set { tbCommand.Text = value; }}
        public string BaksmaliParameter { get { return tbParameters.Text; } set { tbParameters.Text = value; } }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }
    }
}

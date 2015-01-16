using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.ServiceModel.Web;

namespace RestService
{
    public partial class MainForm : Form
    {
        private const string _hostAddress = "http://localhost:9222/RestService/TodoService";
        private WebServiceHost _webServiceHost;

        public MainForm()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, EventArgs e)
        {
            start.Enabled = false;

            message.Text = "Starting";

            var service = new TodoService(this.message);
            _webServiceHost = new WebServiceHost(service, new Uri(_hostAddress));
            _webServiceHost.Open();

            message.Text = "Started";

            stop.Enabled = true;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            stop.Enabled = false;

            message.Text = "Stopping";

            _webServiceHost.Close();
            _webServiceHost = null;

            message.Text = "Stopped";

            start.Enabled = true;
        }
    }
}

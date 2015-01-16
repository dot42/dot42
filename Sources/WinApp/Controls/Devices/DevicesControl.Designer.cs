using Dot42.DeviceLib.UI;

namespace Dot42.Gui.Controls.Devices
{
    partial class DevicesControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.devicesListView = new Dot42.DeviceLib.UI.DevicesListView();
            this.SuspendLayout();
            // 
            // devicesListView
            // 
            this.devicesListView.DeviceMonitorEnabled = false;
            this.devicesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesListView.IsCompatibleCheck = null;
            this.devicesListView.Location = new System.Drawing.Point(8, 8);
            this.devicesListView.Name = "devicesListView";
            this.devicesListView.Size = new System.Drawing.Size(970, 445);
            this.devicesListView.TabIndex = 0;
            this.devicesListView.SelectedDeviceChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // DevicesControl
            // 
            this.Controls.Add(this.devicesListView);
            this.Name = "DevicesControl";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(986, 461);
            this.ResumeLayout(false);

        }

        #endregion

        private DevicesListView devicesListView;
    }
}

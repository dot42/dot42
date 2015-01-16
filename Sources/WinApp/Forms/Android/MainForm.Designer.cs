using System.ComponentModel;
using Dot42.Gui.Controls.Devices;
using Dot42.Shared.UI;

namespace Dot42.Gui.Forms.Android
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the Types of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.modalPanelContainer1 = new Dot42.Shared.UI.ModalPanelContainer();
            this.devicesControl = new Dot42.Gui.Controls.Devices.DevicesControl();
            this.devicesToolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonInstallApk = new System.Windows.Forms.ToolStripButton();
            this.buttonStartActivity = new System.Windows.Forms.ToolStripButton();
            this.buttonLogCat = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonConnectDevice = new System.Windows.Forms.ToolStripButton();
            this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
            this.helpToolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonOpenSamples = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmdGooglePlayDevConsole = new System.Windows.Forms.ToolStripButton();
            this.tabItemDevices = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabItemHelp = new System.Windows.Forms.TabPage();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.lbVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.modalPanelContainer1.SuspendLayout();
            this.devicesToolStrip.SuspendLayout();
            this.helpToolStrip.SuspendLayout();
            this.tabItemDevices.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabItemHelp.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // modalPanelContainer1
            // 
            this.modalPanelContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modalPanelContainer1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.modalPanelContainer1.Controls.Add(this.devicesControl);
            this.modalPanelContainer1.Location = new System.Drawing.Point(0, 70);
            this.modalPanelContainer1.Name = "modalPanelContainer1";
            this.modalPanelContainer1.Size = new System.Drawing.Size(1123, 442);
            this.modalPanelContainer1.TabIndex = 4;
            // 
            // devicesControl
            // 
            this.devicesControl.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.devicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesControl.Location = new System.Drawing.Point(0, 0);
            this.devicesControl.Name = "devicesControl";
            this.devicesControl.Padding = new System.Windows.Forms.Padding(8);
            this.devicesControl.Size = new System.Drawing.Size(1123, 442);
            this.devicesControl.TabIndex = 2;
            this.devicesControl.Text = "devicesControl1";
            // 
            // devicesToolStrip
            // 
            this.devicesToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.devicesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonInstallApk,
            this.buttonStartActivity,
            this.buttonLogCat,
            this.toolStripSeparator1,
            this.buttonConnectDevice,
            this.buttonRefresh});
            this.devicesToolStrip.Location = new System.Drawing.Point(3, 3);
            this.devicesToolStrip.Name = "devicesToolStrip";
            this.devicesToolStrip.Size = new System.Drawing.Size(1109, 39);
            this.devicesToolStrip.TabIndex = 9;
            this.devicesToolStrip.Text = "toolStrip1";
            // 
            // buttonInstallApk
            // 
            this.buttonInstallApk.Image = ((System.Drawing.Image)(resources.GetObject("buttonInstallApk.Image")));
            this.buttonInstallApk.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonInstallApk.Name = "buttonInstallApk";
            this.buttonInstallApk.Size = new System.Drawing.Size(99, 36);
            this.buttonInstallApk.Text = "Install APK";
            // 
            // buttonStartActivity
            // 
            this.buttonStartActivity.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonStartActivity.Image = ((System.Drawing.Image)(resources.GetObject("buttonStartActivity.Image")));
            this.buttonStartActivity.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonStartActivity.Name = "buttonStartActivity";
            this.buttonStartActivity.Size = new System.Drawing.Size(76, 36);
            this.buttonStartActivity.Text = "Start activity";
            this.buttonStartActivity.Visible = false;
            // 
            // buttonLogCat
            // 
            this.buttonLogCat.Image = ((System.Drawing.Image)(resources.GetObject("buttonLogCat.Image")));
            this.buttonLogCat.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonLogCat.Name = "buttonLogCat";
            this.buttonLogCat.Size = new System.Drawing.Size(98, 36);
            this.buttonLogCat.Text = "Device log";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // buttonConnectDevice
            // 
            this.buttonConnectDevice.Image = ((System.Drawing.Image)(resources.GetObject("buttonConnectDevice.Image")));
            this.buttonConnectDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonConnectDevice.Name = "buttonConnectDevice";
            this.buttonConnectDevice.Size = new System.Drawing.Size(132, 36);
            this.buttonConnectDevice.Text = "Connect via WiFi";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("buttonRefresh.Image")));
            this.buttonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(82, 36);
            this.buttonRefresh.Text = "Refresh";
            // 
            // helpToolStrip
            // 
            this.helpToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.helpToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonOpenSamples,
            this.toolStripSeparator2,
            this.cmdGooglePlayDevConsole});
            this.helpToolStrip.Location = new System.Drawing.Point(3, 3);
            this.helpToolStrip.Name = "helpToolStrip";
            this.helpToolStrip.Size = new System.Drawing.Size(1109, 39);
            this.helpToolStrip.TabIndex = 10;
            this.helpToolStrip.Text = "toolStrip2";
            // 
            // buttonOpenSamples
            // 
            this.buttonOpenSamples.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpenSamples.Image")));
            this.buttonOpenSamples.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonOpenSamples.Name = "buttonOpenSamples";
            this.buttonOpenSamples.Size = new System.Drawing.Size(152, 36);
            this.buttonOpenSamples.Text = "Open samples folder";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // cmdGooglePlayDevConsole
            // 
            this.cmdGooglePlayDevConsole.Image = ((System.Drawing.Image)(resources.GetObject("cmdGooglePlayDevConsole.Image")));
            this.cmdGooglePlayDevConsole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdGooglePlayDevConsole.Name = "cmdGooglePlayDevConsole";
            this.cmdGooglePlayDevConsole.Size = new System.Drawing.Size(208, 36);
            this.cmdGooglePlayDevConsole.Text = "Google Play Developer Console";
            this.cmdGooglePlayDevConsole.Click += new System.EventHandler(this.OnPublishAppClick);
            // 
            // tabItemDevices
            // 
            this.tabItemDevices.Controls.Add(this.tabPage1);
            this.tabItemDevices.Controls.Add(this.tabItemHelp);
            this.tabItemDevices.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabItemDevices.Location = new System.Drawing.Point(0, 0);
            this.tabItemDevices.Name = "tabItemDevices";
            this.tabItemDevices.SelectedIndex = 0;
            this.tabItemDevices.Size = new System.Drawing.Size(1123, 70);
            this.tabItemDevices.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.devicesToolStrip);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1115, 44);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "DEVICES";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabItemHelp
            // 
            this.tabItemHelp.Controls.Add(this.helpToolStrip);
            this.tabItemHelp.Location = new System.Drawing.Point(4, 22);
            this.tabItemHelp.Name = "tabItemHelp";
            this.tabItemHelp.Padding = new System.Windows.Forms.Padding(3);
            this.tabItemHelp.Size = new System.Drawing.Size(1115, 44);
            this.tabItemHelp.TabIndex = 1;
            this.tabItemHelp.Text = "HELP";
            this.tabItemHelp.UseVisualStyleBackColor = true;
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbVersion});
            this.statusBar.Location = new System.Drawing.Point(0, 512);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1123, 22);
            this.statusBar.TabIndex = 12;
            this.statusBar.Text = "statusStrip1";
            // 
            // lbVersion
            // 
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(118, 17);
            this.lbVersion.Text = "toolStripStatusLabel1";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1123, 534);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.tabItemDevices);
            this.Controls.Add(this.modalPanelContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "dot42 C# for Android Device Center";
            this.modalPanelContainer1.ResumeLayout(false);
            this.devicesToolStrip.ResumeLayout(false);
            this.devicesToolStrip.PerformLayout();
            this.helpToolStrip.ResumeLayout(false);
            this.helpToolStrip.PerformLayout();
            this.tabItemDevices.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabItemHelp.ResumeLayout(false);
            this.tabItemHelp.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevicesControl devicesControl;
        private ModalPanelContainer modalPanelContainer1;
        private System.Windows.Forms.ToolStrip devicesToolStrip;
        private System.Windows.Forms.ToolStripButton buttonInstallApk;
        private System.Windows.Forms.ToolStripButton buttonStartActivity;
        private System.Windows.Forms.ToolStripButton buttonLogCat;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonConnectDevice;
        private System.Windows.Forms.ToolStripButton buttonRefresh;
        private System.Windows.Forms.ToolStrip helpToolStrip;
        private System.Windows.Forms.ToolStripButton buttonOpenSamples;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton cmdGooglePlayDevConsole;
        private System.Windows.Forms.TabControl tabItemDevices;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabItemHelp;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel lbVersion;
    }
}
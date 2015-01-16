using Dot42.DeviceLib.UI;

namespace Dot42.Ide.Debugger
{
    public partial class DeviceSelectionDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceSelectionDialog));
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.cmdChangeProjectVersion = new System.Windows.Forms.Button();
            this.lbInvalidVersion = new System.Windows.Forms.Label();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.devicesListView = new Dot42.DeviceLib.UI.DevicesListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.miStart = new System.Windows.Forms.ToolStripMenuItem();
            this.lbLoadingDevices = new System.Windows.Forms.Label();
            this.cbUseFromNowOn = new System.Windows.Forms.CheckBox();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tbbConnectDevice = new System.Windows.Forms.ToolStripButton();
            this.tlpMain.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 6;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.cmdChangeProjectVersion, 3, 4);
            this.tlpMain.Controls.Add(this.lbInvalidVersion, 0, 4);
            this.tlpMain.Controls.Add(this.cmdOk, 4, 4);
            this.tlpMain.Controls.Add(this.cmdCancel, 5, 4);
            this.tlpMain.Controls.Add(this.devicesListView, 0, 2);
            this.tlpMain.Controls.Add(this.lbLoadingDevices, 0, 4);
            this.tlpMain.Controls.Add(this.cbUseFromNowOn, 0, 4);
            this.tlpMain.Controls.Add(this.toolBar, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(889, 293);
            this.tlpMain.TabIndex = 0;
            // 
            // cmdChangeProjectVersion
            // 
            this.cmdChangeProjectVersion.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.cmdChangeProjectVersion.Location = new System.Drawing.Point(501, 248);
            this.cmdChangeProjectVersion.Name = "cmdChangeProjectVersion";
            this.cmdChangeProjectVersion.Size = new System.Drawing.Size(160, 39);
            this.cmdChangeProjectVersion.TabIndex = 9;
            this.cmdChangeProjectVersion.Text = "&Yes, change project version";
            this.cmdChangeProjectVersion.UseVisualStyleBackColor = true;
            // 
            // lbInvalidVersion
            // 
            this.lbInvalidVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbInvalidVersion.AutoSize = true;
            this.lbInvalidVersion.Location = new System.Drawing.Point(343, 261);
            this.lbInvalidVersion.Name = "lbInvalidVersion";
            this.lbInvalidVersion.Size = new System.Drawing.Size(74, 13);
            this.lbInvalidVersion.TabIndex = 8;
            this.lbInvalidVersion.Text = "invalid version";
            // 
            // cmdOk
            // 
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(667, 248);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(105, 39);
            this.cmdOk.TabIndex = 4;
            this.cmdOk.Text = "&OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(778, 248);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(105, 39);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // devicesListView
            // 
            this.tlpMain.SetColumnSpan(this.devicesListView, 6);
            this.devicesListView.ContextMenuStrip = this.contextMenuStrip;
            this.devicesListView.DeviceMonitorEnabled = false;
            this.devicesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesListView.IsCompatibleCheck = null;
            this.devicesListView.Location = new System.Drawing.Point(6, 45);
            this.devicesListView.Name = "devicesListView";
            this.devicesListView.Size = new System.Drawing.Size(877, 197);
            this.devicesListView.TabIndex = 1;
            this.devicesListView.SelectedDeviceChanged += new System.EventHandler(this.OnSelectedDeviceChanged);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSelect,
            this.miStart});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(106, 48);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnContextMenuStripOpening);
            // 
            // miSelect
            // 
            this.miSelect.Name = "miSelect";
            this.miSelect.Size = new System.Drawing.Size(105, 22);
            this.miSelect.Text = "&Select";
            this.miSelect.Click += new System.EventHandler(this.OnSelectClick);
            // 
            // miStart
            // 
            this.miStart.Name = "miStart";
            this.miStart.Size = new System.Drawing.Size(105, 22);
            this.miStart.Text = "&Start";
            this.miStart.Click += new System.EventHandler(this.OnStartClick);
            // 
            // lbLoadingDevices
            // 
            this.lbLoadingDevices.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbLoadingDevices.AutoSize = true;
            this.lbLoadingDevices.Location = new System.Drawing.Point(240, 261);
            this.lbLoadingDevices.Name = "lbLoadingDevices";
            this.lbLoadingDevices.Size = new System.Drawing.Size(97, 13);
            this.lbLoadingDevices.TabIndex = 6;
            this.lbLoadingDevices.Text = "Loading devices ...";
            // 
            // cbUseFromNowOn
            // 
            this.cbUseFromNowOn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbUseFromNowOn.AutoSize = true;
            // 
            // 
            // 
            this.cbUseFromNowOn.Checked = true;
            this.cbUseFromNowOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseFromNowOn.Location = new System.Drawing.Point(6, 260);
            this.cbUseFromNowOn.Name = "cbUseFromNowOn";
            this.cbUseFromNowOn.Size = new System.Drawing.Size(228, 15);
            this.cbUseFromNowOn.TabIndex = 10;
            this.cbUseFromNowOn.Text = "Use this device for the rest of this session.";
            this.cbUseFromNowOn.Visible = false;
            // 
            // toolBar
            // 
            this.tlpMain.SetColumnSpan(this.toolBar, 6);
            this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolBar.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tbbRefresh,
            this.tbbConnectDevice});
            this.toolBar.Location = new System.Drawing.Point(3, 3);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(883, 39);
            this.toolBar.TabIndex = 11;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(322, 36);
            this.toolStripLabel1.Text = "Select the device on which you want to debug this package.";
            // 
            // tbbRefresh
            // 
            this.tbbRefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tbbRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tbbRefresh.Image")));
            this.tbbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbbRefresh.Name = "tbbRefresh";
            this.tbbRefresh.Size = new System.Drawing.Size(82, 36);
            this.tbbRefresh.Text = "Refresh";
            this.tbbRefresh.Click += new System.EventHandler(this.OnRefreshClick);
            // 
            // tbbConnectDevice
            // 
            this.tbbConnectDevice.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tbbConnectDevice.Image = ((System.Drawing.Image)(resources.GetObject("tbbConnectDevice.Image")));
            this.tbbConnectDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbbConnectDevice.Name = "tbbConnectDevice";
            this.tbbConnectDevice.Size = new System.Drawing.Size(132, 36);
            this.tbbConnectDevice.Text = "Connect via WiFi";
            this.tbbConnectDevice.Click += new System.EventHandler(this.OnConnectDeviceClick);
            // 
            // DeviceSelectionDialog
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(889, 293);
            this.Controls.Add(this.tlpMain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select device";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lbLoadingDevices;
        private DevicesListView devicesListView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem miSelect;
        private System.Windows.Forms.ToolStripMenuItem miStart;
        private System.Windows.Forms.Button cmdChangeProjectVersion;
        private System.Windows.Forms.Label lbInvalidVersion;
        private System.Windows.Forms.CheckBox cbUseFromNowOn;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.ToolStripButton tbbConnectDevice;
        private System.Windows.Forms.ToolStripButton tbbRefresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    }
}
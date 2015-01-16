
namespace Dot42.DeviceLib.UI
{
    partial class DevicesListView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DevicesListView));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.refreshProgress = new System.Windows.Forms.ProgressBar();
            this.deviceMonitor = new Dot42.AdbLib.DeviceMonitor();
            this.tvList = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSerial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPlatform = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCpuAbi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdStartWizard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(24, 24);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // refreshProgress
            // 
            this.refreshProgress.Location = new System.Drawing.Point(386, 234);
            this.refreshProgress.Maximum = 99;
            this.refreshProgress.Name = "refreshProgress";
            this.refreshProgress.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.refreshProgress.Size = new System.Drawing.Size(219, 23);
            this.refreshProgress.Step = 3;
            this.refreshProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.refreshProgress.TabIndex = 8;
            // 
            // deviceMonitor
            // 
            this.deviceMonitor.Enabled = false;
            // 
            // tvList
            // 
            this.tvList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colType,
            this.colSerial,
            this.colPlatform,
            this.colCpuAbi,
            this.colStatus});
            this.tvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvList.FullRowSelect = true;
            this.tvList.Location = new System.Drawing.Point(0, 0);
            this.tvList.MultiSelect = false;
            this.tvList.Name = "tvList";
            this.tvList.Size = new System.Drawing.Size(1025, 465);
            this.tvList.TabIndex = 2;
            this.tvList.UseCompatibleStateImageBehavior = false;
            this.tvList.View = System.Windows.Forms.View.Details;
            this.tvList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnNodeDoubleClick);
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 150;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 150;
            // 
            // colSerial
            // 
            this.colSerial.Text = "Serial";
            this.colSerial.Width = 150;
            // 
            // colPlatform
            // 
            this.colPlatform.Text = "Platform";
            this.colPlatform.Width = 150;
            // 
            // colCpuAbi
            // 
            this.colCpuAbi.Text = "CPU/ABI";
            this.colCpuAbi.Width = 150;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 150;
            // 
            // cmdStartWizard
            // 
            this.cmdStartWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdStartWizard.AutoSize = true;
            this.cmdStartWizard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdStartWizard.BackColor = System.Drawing.Color.Transparent;
            this.cmdStartWizard.Image = ((System.Drawing.Image)(resources.GetObject("cmdStartWizard.Image")));
            this.cmdStartWizard.Location = new System.Drawing.Point(3, 424);
            this.cmdStartWizard.Name = "cmdStartWizard";
            this.cmdStartWizard.Size = new System.Drawing.Size(223, 38);
            this.cmdStartWizard.TabIndex = 3;
            this.cmdStartWizard.Text = "No devices found. Help me connect!";
            this.cmdStartWizard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdStartWizard.UseVisualStyleBackColor = false;
            this.cmdStartWizard.Click += new System.EventHandler(this.OnStartWizardClick);
            // 
            // DevicesListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdStartWizard);
            this.Controls.Add(this.refreshProgress);
            this.Controls.Add(this.tvList);
            this.Name = "DevicesListView";
            this.Size = new System.Drawing.Size(1025, 465);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AdbLib.DeviceMonitor deviceMonitor;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ProgressBar refreshProgress;
        private System.Windows.Forms.ListView tvList;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colSerial;
        private System.Windows.Forms.ColumnHeader colPlatform;
        private System.Windows.Forms.ColumnHeader colCpuAbi;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Button cmdStartWizard;
    }
}

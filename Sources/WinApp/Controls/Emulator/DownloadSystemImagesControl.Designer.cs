using System.Windows.Forms;

namespace Dot42.Gui.Controls.Emulator
{
    partial class DownloadSystemImagesControl
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
            this.buttonPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cmdDownload = new DevComponents.DotNetBar.ButtonX();
            this.cmdCancel = new DevComponents.DotNetBar.ButtonX();
            this.progress = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.lbProgress = new DevComponents.DotNetBar.LabelX();
            this.lvImages = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chPlatform = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCpu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPanel
            // 
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonPanel.ColumnCount = 4;
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.buttonPanel.Controls.Add(this.cmdDownload, 1, 2);
            this.buttonPanel.Controls.Add(this.cmdCancel, 2, 2);
            this.buttonPanel.Controls.Add(this.progress, 1, 1);
            this.buttonPanel.Controls.Add(this.lbProgress, 0, 0);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(8, 327);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(3);
            this.buttonPanel.RowCount = 3;
            this.buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.buttonPanel.Size = new System.Drawing.Size(970, 126);
            this.buttonPanel.TabIndex = 1;
            // 
            // cmdDownload
            // 
            this.cmdDownload.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDownload.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdDownload.Location = new System.Drawing.Point(375, 83);
            this.cmdDownload.Name = "cmdDownload";
            this.cmdDownload.Size = new System.Drawing.Size(107, 37);
            this.cmdDownload.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdDownload.TabIndex = 0;
            this.cmdDownload.Text = "&Download";
            this.cmdDownload.Click += new System.EventHandler(this.OnDownloadClick);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdCancel.Location = new System.Drawing.Point(488, 83);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(107, 37);
            this.cmdCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // progress
            // 
            // 
            // 
            // 
            this.progress.BackgroundStyle.Class = "";
            this.progress.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.buttonPanel.SetColumnSpan(this.progress, 2);
            this.progress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progress.Location = new System.Drawing.Point(247, 43);
            this.progress.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.progress.Name = "progress";
            this.progress.ProgressType = DevComponents.DotNetBar.eProgressItemType.Marquee;
            this.progress.Size = new System.Drawing.Size(476, 29);
            this.progress.TabIndex = 2;
            this.progress.Text = "Downloading available system images ...";
            // 
            // lbProgress
            // 
            this.lbProgress.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbProgress.AutoSize = true;
            // 
            // 
            // 
            this.lbProgress.BackgroundStyle.Class = "";
            this.lbProgress.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.buttonPanel.SetColumnSpan(this.lbProgress, 4);
            this.lbProgress.Location = new System.Drawing.Point(370, 6);
            this.lbProgress.Name = "lbProgress";
            this.lbProgress.PaddingBottom = 8;
            this.lbProgress.PaddingTop = 8;
            this.lbProgress.Size = new System.Drawing.Size(230, 31);
            this.lbProgress.TabIndex = 3;
            this.lbProgress.Text = "Downloading list of available system images ...";
            // 
            // lvImages
            // 
            this.lvImages.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.lvImages.Border.Class = "ListViewBorder";
            this.lvImages.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lvImages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chPlatform,
            this.chCpu});
            this.lvImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvImages.ForeColor = System.Drawing.Color.Black;
            this.lvImages.FullRowSelect = true;
            this.lvImages.HideSelection = false;
            this.lvImages.Location = new System.Drawing.Point(8, 8);
            this.lvImages.MultiSelect = false;
            this.lvImages.Name = "lvImages";
            this.lvImages.Size = new System.Drawing.Size(970, 319);
            this.lvImages.TabIndex = 2;
            this.lvImages.UseCompatibleStateImageBehavior = false;
            this.lvImages.View = System.Windows.Forms.View.Details;
            this.lvImages.SelectedIndexChanged += new System.EventHandler(this.OnAvdsSelectedIndexChanged);
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 150;
            // 
            // chPlatform
            // 
            this.chPlatform.Text = "Platform";
            this.chPlatform.Width = 150;
            // 
            // chCpu
            // 
            this.chCpu.Text = "CPU/ABI";
            this.chCpu.Width = 150;
            // 
            // DownloadSystemImagesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvImages);
            this.Controls.Add(this.buttonPanel);
            this.Name = "DownloadSystemImagesControl";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(986, 461);
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel buttonPanel;
        private DevComponents.DotNetBar.ButtonX cmdDownload;
        private DevComponents.DotNetBar.ButtonX cmdCancel;
        private DevComponents.DotNetBar.Controls.ListViewEx lvImages;
        private ColumnHeader chName;
        private ColumnHeader chPlatform;
        private ColumnHeader chCpu;
        private DevComponents.DotNetBar.Controls.ProgressBarX progress;
        private DevComponents.DotNetBar.LabelX lbProgress;
    }
}

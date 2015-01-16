namespace Dot42.Ide.Project
{
    partial class AddJarReferenceDialog
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lbJar = new System.Windows.Forms.Label();
            this.tbJarPath = new System.Windows.Forms.TextBox();
            this.cmdBrowseJar = new System.Windows.Forms.Button();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.jarLoader = new System.ComponentModel.BackgroundWorker();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lbJar, 0, 0);
            this.tlpMain.Controls.Add(this.tbJarPath, 1, 0);
            this.tlpMain.Controls.Add(this.cmdBrowseJar, 2, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(3, 3);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(592, 76);
            this.tlpMain.TabIndex = 0;
            // 
            // lbJar
            // 
            this.lbJar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbJar.AutoSize = true;
            this.lbJar.Location = new System.Drawing.Point(6, 11);
            this.lbJar.Name = "lbJar";
            this.lbJar.Size = new System.Drawing.Size(40, 13);
            this.lbJar.TabIndex = 0;
            this.lbJar.Text = "Jar file:";
            // 
            // tbJarPath
            // 
            this.tbJarPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbJarPath.Location = new System.Drawing.Point(52, 6);
            this.tbJarPath.Name = "tbJarPath";
            this.tbJarPath.Size = new System.Drawing.Size(475, 20);
            this.tbJarPath.TabIndex = 1;
            this.tbJarPath.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // cmdBrowseJar
            // 
            this.cmdBrowseJar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdBrowseJar.AutoSize = true;
            this.cmdBrowseJar.Location = new System.Drawing.Point(533, 6);
            this.cmdBrowseJar.Name = "cmdBrowseJar";
            this.cmdBrowseJar.Size = new System.Drawing.Size(53, 23);
            this.cmdBrowseJar.TabIndex = 2;
            this.cmdBrowseJar.Text = "&Browse";
            this.cmdBrowseJar.UseVisualStyleBackColor = true;
            this.cmdBrowseJar.Click += new System.EventHandler(this.OnBrowseJarClick);
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 3);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpButtons.Controls.Add(this.cmdOk, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdCancel, 2, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpButtons.Location = new System.Drawing.Point(6, 40);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpButtons.Size = new System.Drawing.Size(580, 30);
            this.tlpButtons.TabIndex = 8;
            // 
            // cmdOk
            // 
            this.cmdOk.AutoSize = true;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(443, 3);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(64, 24);
            this.cmdOk.TabIndex = 0;
            this.cmdOk.Text = "&OK";
            this.cmdOk.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(513, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(64, 24);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // jarLoader
            // 
            this.jarLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.jarLoader_DoWork);
            this.jarLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.jarLoader_RunWorkerCompleted);
            // 
            // AddJarReferenceDialog
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(598, 81);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddJarReferenceDialog";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Jar Reference...";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lbJar;
        private System.Windows.Forms.TextBox tbJarPath;
        private System.Windows.Forms.Button cmdBrowseJar;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
        private System.ComponentModel.BackgroundWorker jarLoader;
    }
}
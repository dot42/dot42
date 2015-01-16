namespace Dot42.Ide.Debugger
{
    public partial class LauncherDialog
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
            this.pbAttach = new System.Windows.Forms.PictureBox();
            this.pbStart = new System.Windows.Forms.PictureBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.pbDeploy = new System.Windows.Forms.PictureBox();
            this.lbDeploy = new System.Windows.Forms.Label();
            this.lbStart = new System.Windows.Forms.Label();
            this.lbAttach = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAttach)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeploy)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.pbAttach, 0, 2);
            this.tlpMain.Controls.Add(this.pbStart, 0, 1);
            this.tlpMain.Controls.Add(this.cmdCancel, 0, 3);
            this.tlpMain.Controls.Add(this.pbDeploy, 0, 0);
            this.tlpMain.Controls.Add(this.lbDeploy, 1, 0);
            this.tlpMain.Controls.Add(this.lbStart, 1, 1);
            this.tlpMain.Controls.Add(this.lbAttach, 1, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(8);
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(296, 205);
            this.tlpMain.TabIndex = 0;
            // 
            // pbAttach
            // 
            this.pbAttach.Location = new System.Drawing.Point(16, 112);
            this.pbAttach.Margin = new System.Windows.Forms.Padding(8);
            this.pbAttach.Name = "pbAttach";
            this.pbAttach.Size = new System.Drawing.Size(32, 32);
            this.pbAttach.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbAttach.TabIndex = 10;
            this.pbAttach.TabStop = false;
            // 
            // pbStart
            // 
            this.pbStart.Location = new System.Drawing.Point(16, 64);
            this.pbStart.Margin = new System.Windows.Forms.Padding(8);
            this.pbStart.Name = "pbStart";
            this.pbStart.Size = new System.Drawing.Size(32, 32);
            this.pbStart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStart.TabIndex = 8;
            this.pbStart.TabStop = false;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cmdCancel, 2);
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(180, 155);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(105, 39);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // pbDeploy
            // 
            this.pbDeploy.Location = new System.Drawing.Point(16, 16);
            this.pbDeploy.Margin = new System.Windows.Forms.Padding(8);
            this.pbDeploy.Name = "pbDeploy";
            this.pbDeploy.Size = new System.Drawing.Size(32, 32);
            this.pbDeploy.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbDeploy.TabIndex = 6;
            this.pbDeploy.TabStop = false;
            // 
            // lbDeploy
            // 
            this.lbDeploy.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbDeploy.AutoSize = true;
            this.lbDeploy.Location = new System.Drawing.Point(59, 25);
            this.lbDeploy.Name = "lbDeploy";
            this.lbDeploy.Size = new System.Drawing.Size(122, 13);
            this.lbDeploy.TabIndex = 7;
            this.lbDeploy.Text = "Deploying app to device";
            // 
            // lbStart
            // 
            this.lbStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbStart.AutoSize = true;
            this.lbStart.Location = new System.Drawing.Point(59, 73);
            this.lbStart.Name = "lbStart";
            this.lbStart.Size = new System.Drawing.Size(64, 13);
            this.lbStart.TabIndex = 9;
            this.lbStart.Text = "Starting app";
            // 
            // lbAttach
            // 
            this.lbAttach.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbAttach.AutoSize = true;
            this.lbAttach.Location = new System.Drawing.Point(59, 121);
            this.lbAttach.Name = "lbAttach";
            this.lbAttach.Size = new System.Drawing.Size(100, 13);
            this.lbAttach.TabIndex = 11;
            this.lbAttach.Text = "Attaching debugger";
            // 
            // LauncherDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(296, 201);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LauncherDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Starting {0}";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAttach)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeploy)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox pbDeploy;
        private System.Windows.Forms.PictureBox pbAttach;
        private System.Windows.Forms.PictureBox pbStart;
        private System.Windows.Forms.Label lbDeploy;
        private System.Windows.Forms.Label lbStart;
        private System.Windows.Forms.Label lbAttach;
    }
}
namespace Dot42.Debugger.Forms
{
    partial class AttachForm
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
            this.tbPid = new System.Windows.Forms.NumericUpDown();
            this.lbPid = new System.Windows.Forms.Label();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tbPid)).BeginInit();
            this.SuspendLayout();
            // 
            // tbPid
            // 
            // 
            // 
            // 
            this.tbPid.Location = new System.Drawing.Point(96, 16);
            this.tbPid.Maximum = 6400000;
            this.tbPid.Minimum = 0;
            this.tbPid.Name = "tbPid";
            this.tbPid.Size = new System.Drawing.Size(208, 20);
            this.tbPid.TabIndex = 0;
            this.tbPid.ValueChanged += new System.EventHandler(this.tbPid_ValueChanged);
            // 
            // lbPid
            // 
            this.lbPid.AutoSize = true;
            // 
            // 
            // 
            this.lbPid.Location = new System.Drawing.Point(16, 16);
            this.lbPid.Name = "lbPid";
            this.lbPid.Size = new System.Drawing.Size(56, 15);
            this.lbPid.TabIndex = 1;
            this.lbPid.Text = "Process ID";
            // 
            // cmdOk
            // 
            this.cmdOk.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(104, 56);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(88, 32);
            this.cmdOk.TabIndex = 2;
            this.cmdOk.Text = "&OK";
            // 
            // cmdCancel
            // 
            this.cmdCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCancel.Location = new System.Drawing.Point(200, 56);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(96, 32);
            this.cmdCancel.TabIndex = 3;
            this.cmdCancel.Text = "&Cancel";
            // 
            // AttachForm
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(310, 98);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.lbPid);
            this.Controls.Add(this.tbPid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttachForm";
            this.Text = "Attach to process";
            ((System.ComponentModel.ISupportInitialize)(this.tbPid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown tbPid;
        private System.Windows.Forms.Label lbPid;
        private System.Windows.Forms.Button cmdOk;
        private System.Windows.Forms.Button cmdCancel;
    }
}
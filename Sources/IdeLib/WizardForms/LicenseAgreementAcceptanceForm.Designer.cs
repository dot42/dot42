namespace Dot42.Ide.WizardForms
{
    partial class LicenseAgreementAcceptanceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseAgreementAcceptanceForm));
            this.tvMain = new System.Windows.Forms.TableLayoutPanel();
            this.cmdAgree = new System.Windows.Forms.Button();
            this.cmdNotAgree = new System.Windows.Forms.Button();
            this.lbHeader = new System.Windows.Forms.Label();
            this.lbLibraries = new System.Windows.Forms.ListBox();
            this.tbAgreement = new System.Windows.Forms.TextBox();
            this.tvMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvMain
            // 
            this.tvMain.ColumnCount = 4;
            this.tvMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tvMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tvMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tvMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tvMain.Controls.Add(this.cmdAgree, 2, 2);
            this.tvMain.Controls.Add(this.cmdNotAgree, 3, 2);
            this.tvMain.Controls.Add(this.lbHeader, 0, 0);
            this.tvMain.Controls.Add(this.lbLibraries, 0, 1);
            this.tvMain.Controls.Add(this.tbAgreement, 1, 1);
            this.tvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvMain.Location = new System.Drawing.Point(0, 0);
            this.tvMain.Name = "tvMain";
            this.tvMain.RowCount = 3;
            this.tvMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tvMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tvMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tvMain.Size = new System.Drawing.Size(634, 359);
            this.tvMain.TabIndex = 0;
            // 
            // cmdAgree
            // 
            this.cmdAgree.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdAgree.Location = new System.Drawing.Point(379, 317);
            this.cmdAgree.Margin = new System.Windows.Forms.Padding(8);
            this.cmdAgree.Name = "cmdAgree";
            this.cmdAgree.Size = new System.Drawing.Size(116, 34);
            this.cmdAgree.TabIndex = 3;
            this.cmdAgree.Text = "I &agree";
            this.cmdAgree.UseVisualStyleBackColor = true;
            // 
            // cmdNotAgree
            // 
            this.cmdNotAgree.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdNotAgree.Location = new System.Drawing.Point(511, 317);
            this.cmdNotAgree.Margin = new System.Windows.Forms.Padding(8);
            this.cmdNotAgree.Name = "cmdNotAgree";
            this.cmdNotAgree.Size = new System.Drawing.Size(115, 34);
            this.cmdNotAgree.TabIndex = 4;
            this.cmdNotAgree.Text = "I do &not agree";
            this.cmdNotAgree.UseVisualStyleBackColor = true;
            // 
            // lbHeader
            // 
            this.lbHeader.AutoSize = true;
            this.tvMain.SetColumnSpan(this.lbHeader, 4);
            this.lbHeader.Location = new System.Drawing.Point(8, 8);
            this.lbHeader.Margin = new System.Windows.Forms.Padding(8);
            this.lbHeader.Name = "lbHeader";
            this.lbHeader.Size = new System.Drawing.Size(380, 13);
            this.lbHeader.TabIndex = 0;
            this.lbHeader.Text = "Before using these libraries you must accept to the following license agreement.";
            // 
            // lbLibraries
            // 
            this.lbLibraries.Dock = System.Windows.Forms.DockStyle.Left;
            this.lbLibraries.FormattingEnabled = true;
            this.lbLibraries.Location = new System.Drawing.Point(8, 37);
            this.lbLibraries.Margin = new System.Windows.Forms.Padding(8);
            this.lbLibraries.Name = "lbLibraries";
            this.lbLibraries.Size = new System.Drawing.Size(184, 264);
            this.lbLibraries.TabIndex = 1;
            // 
            // tbAgreement
            // 
            this.tvMain.SetColumnSpan(this.tbAgreement, 3);
            this.tbAgreement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAgreement.Location = new System.Drawing.Point(208, 37);
            this.tbAgreement.Margin = new System.Windows.Forms.Padding(8);
            this.tbAgreement.MaxLength = 3000000;
            this.tbAgreement.Multiline = true;
            this.tbAgreement.Name = "tbAgreement";
            this.tbAgreement.ReadOnly = true;
            this.tbAgreement.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbAgreement.Size = new System.Drawing.Size(418, 264);
            this.tbAgreement.TabIndex = 2;
            // 
            // LicenseAgreementAcceptanceForm
            // 
            this.AcceptButton = this.cmdAgree;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdNotAgree;
            this.ClientSize = new System.Drawing.Size(634, 359);
            this.Controls.Add(this.tvMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LicenseAgreementAcceptanceForm";
            this.Text = "License agreement";
            this.tvMain.ResumeLayout(false);
            this.tvMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tvMain;
        private System.Windows.Forms.Button cmdAgree;
        private System.Windows.Forms.Button cmdNotAgree;
        private System.Windows.Forms.Label lbHeader;
        private System.Windows.Forms.ListBox lbLibraries;
        private System.Windows.Forms.TextBox tbAgreement;
    }
}

namespace Dot42.Ide.WizardForms
{
    public partial class ApplicationProjectWizardDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationProjectWizardDialog));
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdOpenCertificate = new System.Windows.Forms.Button();
            this.lbVersion = new System.Windows.Forms.Label();
            this.cbFramework = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lbHeader = new System.Windows.Forms.Label();
            this.cmdCreate = new System.Windows.Forms.Button();
            this.tbCertificatePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.additionalLibrariesControl1 = new Dot42.Ide.WizardForms.AdditionalLibrariesControl();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.label1, 0, 1);
            this.tlpMain.Controls.Add(this.cmdOpenCertificate, 2, 1);
            this.tlpMain.Controls.Add(this.lbVersion, 0, 2);
            this.tlpMain.Controls.Add(this.cbFramework, 1, 2);
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 4);
            this.tlpMain.Controls.Add(this.lbHeader, 0, 0);
            this.tlpMain.Controls.Add(this.cmdCreate, 3, 1);
            this.tlpMain.Controls.Add(this.tbCertificatePath, 1, 1);
            this.tlpMain.Controls.Add(this.additionalLibrariesControl1, 1, 3);
            this.tlpMain.Controls.Add(this.label2, 0, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(600, 251);
            this.tlpMain.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Certificate:";
            // 
            // cmdOpenCertificate
            // 
            this.cmdOpenCertificate.AutoSize = true;
            this.cmdOpenCertificate.Image = ((System.Drawing.Image)(resources.GetObject("cmdOpenCertificate.Image")));
            this.cmdOpenCertificate.Location = new System.Drawing.Point(508, 41);
            this.cmdOpenCertificate.Name = "cmdOpenCertificate";
            this.cmdOpenCertificate.Size = new System.Drawing.Size(40, 29);
            this.cmdOpenCertificate.TabIndex = 3;
            this.cmdOpenCertificate.UseVisualStyleBackColor = true;
            this.cmdOpenCertificate.Click += new System.EventHandler(this.cmdOpenCertificate_Click);
            // 
            // lbVersion
            // 
            this.lbVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbVersion.AutoSize = true;
            this.lbVersion.Location = new System.Drawing.Point(6, 80);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(134, 13);
            this.lbVersion.TabIndex = 5;
            this.lbVersion.Text = "Target Framework Version:";
            // 
            // cbFramework
            // 
            this.tlpMain.SetColumnSpan(this.cbFramework, 3);
            this.cbFramework.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFramework.FormattingEnabled = true;
            this.cbFramework.Location = new System.Drawing.Point(146, 76);
            this.cbFramework.Name = "cbFramework";
            this.cbFramework.Size = new System.Drawing.Size(448, 21);
            this.cbFramework.TabIndex = 6;
            this.cbFramework.SelectedIndexChanged += new System.EventHandler(this.OnTargetFrameworkVersionChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tlpMain.SetColumnSpan(this.tableLayoutPanel1, 4);
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.cmdOK, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdCancel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 210);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(588, 35);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(403, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(88, 29);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "&OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.OnOkClick);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(497, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(88, 29);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // lbHeader
            // 
            this.lbHeader.AutoSize = true;
            // 
            // 
            // 
            this.tlpMain.SetColumnSpan(this.lbHeader, 4);
            this.lbHeader.Location = new System.Drawing.Point(6, 6);
            this.lbHeader.Name = "lbHeader";
            this.lbHeader.Size = new System.Drawing.Size(588, 29);
            this.lbHeader.TabIndex = 0;
            this.lbHeader.Text = "To create a new dot42 application project, you must select the target Android fra" +
    "mework version and select or create a certificate. This certificate is used to s" +
    "ign the resulting Android package.";
            // 
            // cmdCreate
            // 
            this.cmdCreate.AutoSize = true;
            this.cmdCreate.Image = ((System.Drawing.Image)(resources.GetObject("cmdCreate.Image")));
            this.cmdCreate.Location = new System.Drawing.Point(554, 41);
            this.cmdCreate.Name = "cmdCreate";
            this.cmdCreate.Size = new System.Drawing.Size(40, 29);
            this.cmdCreate.TabIndex = 4;
            this.cmdCreate.UseVisualStyleBackColor = true;
            this.cmdCreate.Click += new System.EventHandler(this.OnCreateCertificateClick);
            // 
            // tbCertificatePath
            // 
            this.tbCertificatePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCertificatePath.Enabled = false;
            this.tbCertificatePath.Location = new System.Drawing.Point(146, 45);
            this.tbCertificatePath.Name = "tbCertificatePath";
            this.tbCertificatePath.Size = new System.Drawing.Size(356, 20);
            this.tbCertificatePath.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 106);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Additional libraries:";
            // 
            // additionalLibrariesControl1
            // 
            this.tlpMain.SetColumnSpan(this.additionalLibrariesControl1, 3);
            this.additionalLibrariesControl1.Location = new System.Drawing.Point(146, 103);
            this.additionalLibrariesControl1.Name = "additionalLibrariesControl1";
            this.additionalLibrariesControl1.Size = new System.Drawing.Size(448, 94);
            this.additionalLibrariesControl1.TabIndex = 8;
            // 
            // ApplicationProjectWizardDialog
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(600, 252);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplicationProjectWizardDialog";
            this.Text = "dot42 Application Project Wizard";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdOpenCertificate;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.ComboBox cbFramework;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdCreate;
        private System.Windows.Forms.Label lbHeader;
        private System.Windows.Forms.TextBox tbCertificatePath;
        private AdditionalLibrariesControl additionalLibrariesControl1;
        private System.Windows.Forms.Label label2;
    }
}
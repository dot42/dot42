using Dot42.CryptoUI.Controls;

namespace Dot42.CryptoUI
{
    partial class CertificateWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertificateWizard));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.wizard = new System.Windows.Forms.TabControl();
            this.welcomePage = new System.Windows.Forms.TabPage();
            this.detailPage = new System.Windows.Forms.TabPage();
            this.passwordPage = new System.Windows.Forms.TabPage();
            this.storagePage = new System.Windows.Forms.TabPage();
            this.creatingPage = new System.Windows.Forms.TabPage();
            this.nextButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.settingsControl = new Dot42.CryptoUI.Controls.CertSettingsControl();
            this.passwordControl = new Dot42.CryptoUI.Controls.KeyStoreSettingsControl();
            this.saveAsControl = new Dot42.CryptoUI.Controls.SaveAsControl();
            this.creatingCertificateControl = new Dot42.CryptoUI.Controls.CreatingCertificateControl();
            this.wizard.SuspendLayout();
            this.welcomePage.SuspendLayout();
            this.detailPage.SuspendLayout();
            this.passwordPage.SuspendLayout();
            this.storagePage.SuspendLayout();
            this.creatingPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 16F);
            this.label1.Location = new System.Drawing.Point(210, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(505, 66);
            this.label1.TabIndex = 0;
            this.label1.Text = "Welcome to the Certificate Wizard";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(210, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(504, 180);
            this.label2.TabIndex = 1;
            this.label2.Text = "This wizard will assist you to create a self-signed certificate that can be used " +
    "to sign Android packages.";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(210, 289);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "To continue, click Next.";
            // 
            // wizard
            // 
            this.wizard.Controls.Add(this.welcomePage);
            this.wizard.Controls.Add(this.detailPage);
            this.wizard.Controls.Add(this.passwordPage);
            this.wizard.Controls.Add(this.storagePage);
            this.wizard.Controls.Add(this.creatingPage);
            this.wizard.ItemSize = new System.Drawing.Size(57, 18);
            this.wizard.Location = new System.Drawing.Point(0, 0);
            this.wizard.Name = "wizard";
            this.wizard.SelectedIndex = 0;
            this.wizard.Size = new System.Drawing.Size(729, 339);
            this.wizard.TabIndex = 1;
            this.wizard.SelectedIndexChanged += new System.EventHandler(this.OnWizardPageChanged);
            this.wizard.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.AllowTabToChange);
            // 
            // welcomePage
            // 
            this.welcomePage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("welcomePage.BackgroundImage")));
            this.welcomePage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.welcomePage.Controls.Add(this.label3);
            this.welcomePage.Controls.Add(this.label2);
            this.welcomePage.Controls.Add(this.label1);
            this.welcomePage.Location = new System.Drawing.Point(4, 22);
            this.welcomePage.Name = "welcomePage";
            this.welcomePage.Padding = new System.Windows.Forms.Padding(3);
            this.welcomePage.Size = new System.Drawing.Size(721, 313);
            this.welcomePage.TabIndex = 0;
            this.welcomePage.Text = "Welcome";
            this.welcomePage.UseVisualStyleBackColor = true;
            // 
            // detailPage
            // 
            this.detailPage.Controls.Add(this.settingsControl);
            this.detailPage.Location = new System.Drawing.Point(4, 22);
            this.detailPage.Name = "detailPage";
            this.detailPage.Padding = new System.Windows.Forms.Padding(3);
            this.detailPage.Size = new System.Drawing.Size(721, 313);
            this.detailPage.TabIndex = 1;
            this.detailPage.Text = "Details";
            this.detailPage.UseVisualStyleBackColor = true;
            // 
            // passwordPage
            // 
            this.passwordPage.Controls.Add(this.passwordControl);
            this.passwordPage.Location = new System.Drawing.Point(4, 22);
            this.passwordPage.Name = "passwordPage";
            this.passwordPage.Size = new System.Drawing.Size(721, 313);
            this.passwordPage.TabIndex = 2;
            this.passwordPage.Text = "Password";
            this.passwordPage.UseVisualStyleBackColor = true;
            // 
            // storagePage
            // 
            this.storagePage.Controls.Add(this.saveAsControl);
            this.storagePage.Location = new System.Drawing.Point(4, 22);
            this.storagePage.Name = "storagePage";
            this.storagePage.Size = new System.Drawing.Size(721, 313);
            this.storagePage.TabIndex = 3;
            this.storagePage.Text = "Storage";
            this.storagePage.UseVisualStyleBackColor = true;
            // 
            // creatingPage
            // 
            this.creatingPage.Controls.Add(this.creatingCertificateControl);
            this.creatingPage.Location = new System.Drawing.Point(4, 22);
            this.creatingPage.Name = "creatingPage";
            this.creatingPage.Size = new System.Drawing.Size(721, 313);
            this.creatingPage.TabIndex = 4;
            this.creatingPage.Text = "Creating";
            this.creatingPage.UseVisualStyleBackColor = true;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(559, 345);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.NextButtonClicked);
            // 
            // backButton
            // 
            this.backButton.Enabled = false;
            this.backButton.Location = new System.Drawing.Point(478, 345);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(75, 23);
            this.backButton.TabIndex = 2;
            this.backButton.Text = "Back";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.BackButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(640, 345);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
            // 
            // settingsControl
            // 
            this.settingsControl.BackColor = System.Drawing.SystemColors.Control;
            this.settingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsControl.Location = new System.Drawing.Point(3, 3);
            this.settingsControl.Name = "settingsControl";
            this.settingsControl.Size = new System.Drawing.Size(715, 307);
            this.settingsControl.TabIndex = 0;
            // 
            // passwordControl
            // 
            this.passwordControl.BackColor = System.Drawing.SystemColors.Control;
            this.passwordControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.passwordControl.Location = new System.Drawing.Point(0, 0);
            this.passwordControl.Name = "passwordControl";
            this.passwordControl.Size = new System.Drawing.Size(721, 313);
            this.passwordControl.TabIndex = 0;
            // 
            // saveAsControl
            // 
            this.saveAsControl.BackColor = System.Drawing.SystemColors.Control;
            this.saveAsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveAsControl.FileName = "";
            this.saveAsControl.Location = new System.Drawing.Point(0, 0);
            this.saveAsControl.Name = "saveAsControl";
            this.saveAsControl.Size = new System.Drawing.Size(721, 313);
            this.saveAsControl.TabIndex = 0;
            // 
            // creatingCertificateControl
            // 
            this.creatingCertificateControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.creatingCertificateControl.Location = new System.Drawing.Point(0, 0);
            this.creatingCertificateControl.Name = "creatingCertificateControl";
            this.creatingCertificateControl.Size = new System.Drawing.Size(721, 313);
            this.creatingCertificateControl.TabIndex = 0;
            // 
            // CertificateWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 380);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.wizard);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CertificateWizard";
            this.Text = "Certificate Wizard";
            this.wizard.ResumeLayout(false);
            this.welcomePage.ResumeLayout(false);
            this.detailPage.ResumeLayout(false);
            this.passwordPage.ResumeLayout(false);
            this.storagePage.ResumeLayout(false);
            this.creatingPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl wizard;
        private System.Windows.Forms.TabPage welcomePage;
        private System.Windows.Forms.TabPage detailPage;
        private System.Windows.Forms.TabPage passwordPage;
        private System.Windows.Forms.TabPage storagePage;
        private System.Windows.Forms.TabPage creatingPage;
        private CertSettingsControl settingsControl;
        private KeyStoreSettingsControl passwordControl;
        private SaveAsControl saveAsControl;
        private CreatingCertificateControl creatingCertificateControl;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
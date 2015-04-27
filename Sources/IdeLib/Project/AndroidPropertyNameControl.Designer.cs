namespace Dot42.VStudio.Flavors
{
    public partial class AndroidPropertyNameControl
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.cbGenerateSetNextInstructionCode = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbPackageName = new System.Windows.Forms.TextBox();
            this.lbApkFilename = new System.Windows.Forms.Label();
            this.tbApkFilename = new System.Windows.Forms.TextBox();
            this.cbAndroidVersion = new System.Windows.Forms.ComboBox();
            this.lbCertificate = new System.Windows.Forms.Label();
            this.cmdBrowseCertificate = new System.Windows.Forms.Button();
            this.cmdNewCertificate = new System.Windows.Forms.Button();
            this.lbAndroidVersionInfo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbGenerateWcfProxy = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbGenerateWcfProxy = new System.Windows.Forms.CheckBox();
            this.tbCertificate = new System.Windows.Forms.TextBox();
            this.lbFramework = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbRootNamespace = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbAssemblyName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.labelSetNextInstructionHelp = new System.Windows.Forms.Label();
            this.cbTargetSdkVersion = new System.Windows.Forms.ComboBox();
            this.lbTargetSdkVersion = new System.Windows.Forms.Label();
            this.additionalLibrariesControl = new Dot42.Ide.WizardForms.AdditionalLibrariesControl();
            this.tlpMain.SuspendLayout();
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
            this.tlpMain.Controls.Add(this.cbGenerateSetNextInstructionCode, 1, 16);
            this.tlpMain.Controls.Add(this.label1, 0, 0);
            this.tlpMain.Controls.Add(this.tbPackageName, 1, 0);
            this.tlpMain.Controls.Add(this.lbApkFilename, 0, 1);
            this.tlpMain.Controls.Add(this.tbApkFilename, 1, 1);
            this.tlpMain.Controls.Add(this.cbAndroidVersion, 1, 5);
            this.tlpMain.Controls.Add(this.lbCertificate, 0, 10);
            this.tlpMain.Controls.Add(this.cmdBrowseCertificate, 2, 10);
            this.tlpMain.Controls.Add(this.cmdNewCertificate, 3, 10);
            this.tlpMain.Controls.Add(this.lbAndroidVersionInfo, 1, 6);
            this.tlpMain.Controls.Add(this.label2, 1, 8);
            this.tlpMain.Controls.Add(this.additionalLibrariesControl, 1, 14);
            this.tlpMain.Controls.Add(this.lbGenerateWcfProxy, 0, 12);
            this.tlpMain.Controls.Add(this.label3, 1, 13);
            this.tlpMain.Controls.Add(this.cbGenerateWcfProxy, 1, 12);
            this.tlpMain.Controls.Add(this.tbCertificate, 1, 10);
            this.tlpMain.Controls.Add(this.lbFramework, 0, 5);
            this.tlpMain.Controls.Add(this.label4, 0, 14);
            this.tlpMain.Controls.Add(this.label5, 0, 2);
            this.tlpMain.Controls.Add(this.tbRootNamespace, 1, 2);
            this.tlpMain.Controls.Add(this.label7, 0, 3);
            this.tlpMain.Controls.Add(this.tbAssemblyName, 1, 3);
            this.tlpMain.Controls.Add(this.label8, 0, 16);
            this.tlpMain.Controls.Add(this.labelSetNextInstructionHelp, 1, 17);
            this.tlpMain.Controls.Add(this.cbTargetSdkVersion, 1, 7);
            this.tlpMain.Controls.Add(this.lbTargetSdkVersion, 0, 7);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 17;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(500, 544);
            this.tlpMain.TabIndex = 0;
            // 
            // cbGenerateSetNextInstructionCode
            // 
            this.cbGenerateSetNextInstructionCode.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.cbGenerateSetNextInstructionCode, 3);
            this.cbGenerateSetNextInstructionCode.Location = new System.Drawing.Point(147, 490);
            this.cbGenerateSetNextInstructionCode.Name = "cbGenerateSetNextInstructionCode";
            this.cbGenerateSetNextInstructionCode.Size = new System.Drawing.Size(330, 17);
            this.cbGenerateSetNextInstructionCode.TabIndex = 51;
            this.cbGenerateSetNextInstructionCode.Text = "Generate code to enable \"set next instruction\" during debugging";
            this.cbGenerateSetNextInstructionCode.UseVisualStyleBackColor = true;
            this.cbGenerateSetNextInstructionCode.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Package name:";
            // 
            // tbPackageName
            // 
            this.tbPackageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.tbPackageName, 3);
            this.tbPackageName.Location = new System.Drawing.Point(147, 3);
            this.tbPackageName.Name = "tbPackageName";
            this.tbPackageName.Size = new System.Drawing.Size(350, 20);
            this.tbPackageName.TabIndex = 1;
            this.tbPackageName.TextChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // lbApkFilename
            // 
            this.lbApkFilename.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbApkFilename.AutoSize = true;
            this.lbApkFilename.Location = new System.Drawing.Point(3, 32);
            this.lbApkFilename.Name = "lbApkFilename";
            this.lbApkFilename.Size = new System.Drawing.Size(98, 13);
            this.lbApkFilename.TabIndex = 2;
            this.lbApkFilename.Text = "Package file name:";
            // 
            // tbApkFilename
            // 
            this.tbApkFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.tbApkFilename, 3);
            this.tbApkFilename.Location = new System.Drawing.Point(147, 29);
            this.tbApkFilename.Name = "tbApkFilename";
            this.tbApkFilename.Size = new System.Drawing.Size(350, 20);
            this.tbApkFilename.TabIndex = 3;
            this.tbApkFilename.TextChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // cbAndroidVersion
            // 
            this.tlpMain.SetColumnSpan(this.cbAndroidVersion, 3);
            this.cbAndroidVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbAndroidVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAndroidVersion.FormattingEnabled = true;
            this.cbAndroidVersion.Location = new System.Drawing.Point(147, 127);
            this.cbAndroidVersion.Name = "cbAndroidVersion";
            this.cbAndroidVersion.Size = new System.Drawing.Size(350, 21);
            this.cbAndroidVersion.TabIndex = 11;
            this.cbAndroidVersion.SelectedIndexChanged += new System.EventHandler(this.OnAndroidVersionSelectedIndexChanged);
            this.cbAndroidVersion.SelectedValueChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // lbCertificate
            // 
            this.lbCertificate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbCertificate.AutoSize = true;
            this.lbCertificate.Location = new System.Drawing.Point(3, 267);
            this.lbCertificate.Name = "lbCertificate";
            this.lbCertificate.Size = new System.Drawing.Size(57, 13);
            this.lbCertificate.TabIndex = 20;
            this.lbCertificate.Text = "Certificate:";
            // 
            // cmdBrowseCertificate
            // 
            this.cmdBrowseCertificate.Location = new System.Drawing.Point(427, 261);
            this.cmdBrowseCertificate.Name = "cmdBrowseCertificate";
            this.cmdBrowseCertificate.Size = new System.Drawing.Size(32, 26);
            this.cmdBrowseCertificate.TabIndex = 22;
            this.cmdBrowseCertificate.UseVisualStyleBackColor = true;
            this.cmdBrowseCertificate.Click += new System.EventHandler(this.OnBrowseCertificateClick);
            // 
            // cmdNewCertificate
            // 
            this.cmdNewCertificate.Location = new System.Drawing.Point(465, 261);
            this.cmdNewCertificate.Name = "cmdNewCertificate";
            this.cmdNewCertificate.Size = new System.Drawing.Size(32, 26);
            this.cmdNewCertificate.TabIndex = 23;
            this.cmdNewCertificate.UseVisualStyleBackColor = true;
            this.cmdNewCertificate.Click += new System.EventHandler(this.OnNewCertificateClick);
            // 
            // lbAndroidVersionInfo
            // 
            this.lbAndroidVersionInfo.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lbAndroidVersionInfo, 3);
            this.lbAndroidVersionInfo.Location = new System.Drawing.Point(147, 151);
            this.lbAndroidVersionInfo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.lbAndroidVersionInfo.Name = "lbAndroidVersionInfo";
            this.lbAndroidVersionInfo.Size = new System.Drawing.Size(284, 26);
            this.lbAndroidVersionInfo.TabIndex = 12;
            this.lbAndroidVersionInfo.Text = "The minimum required Android version of the target device.\r\nCorresponds to androi" +
    "d:minSdkVersion attribute.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.label2, 3);
            this.label2.Location = new System.Drawing.Point(147, 212);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(289, 26);
            this.label2.TabIndex = 15;
            this.label2.Text = "The Android version against which this software was tested.\r\nCorresponds to the a" +
    "ndroid:targetSdkVersion attribute.";
            // 
            // lbGenerateWcfProxy
            // 
            this.lbGenerateWcfProxy.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbGenerateWcfProxy.AutoSize = true;
            this.lbGenerateWcfProxy.Location = new System.Drawing.Point(3, 315);
            this.lbGenerateWcfProxy.Name = "lbGenerateWcfProxy";
            this.lbGenerateWcfProxy.Size = new System.Drawing.Size(56, 13);
            this.lbGenerateWcfProxy.TabIndex = 30;
            this.lbGenerateWcfProxy.Text = "Use WCF:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.label3, 3);
            this.label3.Location = new System.Drawing.Point(147, 333);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(307, 26);
            this.label3.TabIndex = 32;
            this.label3.Text = "Use this property to ensure that the proxy (needed at runtime) is generated and i" +
    "ncluded in your package.";
            // 
            // cbGenerateWcfProxy
            // 
            this.cbGenerateWcfProxy.AutoSize = true;
            this.cbGenerateWcfProxy.Location = new System.Drawing.Point(147, 313);
            this.cbGenerateWcfProxy.Name = "cbGenerateWcfProxy";
            this.cbGenerateWcfProxy.Size = new System.Drawing.Size(137, 17);
            this.cbGenerateWcfProxy.TabIndex = 31;
            this.cbGenerateWcfProxy.Text = "Generate a WCF proxy.";
            this.cbGenerateWcfProxy.UseVisualStyleBackColor = true;
            this.cbGenerateWcfProxy.CheckedChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // tbCertificate
            // 
            this.tbCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCertificate.Location = new System.Drawing.Point(147, 264);
            this.tbCertificate.Name = "tbCertificate";
            this.tbCertificate.ReadOnly = true;
            this.tbCertificate.Size = new System.Drawing.Size(274, 20);
            this.tbCertificate.TabIndex = 21;
            // 
            // lbFramework
            // 
            this.lbFramework.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbFramework.AutoSize = true;
            this.lbFramework.Location = new System.Drawing.Point(3, 131);
            this.lbFramework.Name = "lbFramework";
            this.lbFramework.Size = new System.Drawing.Size(83, 13);
            this.lbFramework.TabIndex = 10;
            this.lbFramework.Text = "Android version:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 367);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "Use libraries";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Root namespace:";
            // 
            // tbRootNamespace
            // 
            this.tbRootNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.tbRootNamespace, 3);
            this.tbRootNamespace.Location = new System.Drawing.Point(147, 55);
            this.tbRootNamespace.Name = "tbRootNamespace";
            this.tbRootNamespace.Size = new System.Drawing.Size(350, 20);
            this.tbRootNamespace.TabIndex = 5;
            this.tbRootNamespace.TextChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Assembly name:";
            // 
            // tbAssemblyName
            // 
            this.tbAssemblyName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.tbAssemblyName, 3);
            this.tbAssemblyName.Location = new System.Drawing.Point(147, 81);
            this.tbAssemblyName.Name = "tbAssemblyName";
            this.tbAssemblyName.Size = new System.Drawing.Size(350, 20);
            this.tbAssemblyName.TabIndex = 7;
            this.tbAssemblyName.TextChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 492);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 13);
            this.label8.TabIndex = 50;
            this.label8.Text = "Debugging:";
            // 
            // labelSetNextInstructionHelp
            // 
            this.labelSetNextInstructionHelp.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.labelSetNextInstructionHelp, 3);
            this.labelSetNextInstructionHelp.Location = new System.Drawing.Point(147, 510);
            this.labelSetNextInstructionHelp.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.labelSetNextInstructionHelp.Name = "labelSetNextInstructionHelp";
            this.labelSetNextInstructionHelp.Size = new System.Drawing.Size(321, 26);
            this.labelSetNextInstructionHelp.TabIndex = 52;
            this.labelSetNextInstructionHelp.Text = "Note that this will increase the .apk size and might slow down your application. " +
    "This setting is ignored during release builds.";
            // 
            // cbTargetSdkVersion
            // 
            this.tlpMain.SetColumnSpan(this.cbTargetSdkVersion, 3);
            this.cbTargetSdkVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbTargetSdkVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTargetSdkVersion.FormattingEnabled = true;
            this.cbTargetSdkVersion.Location = new System.Drawing.Point(147, 188);
            this.cbTargetSdkVersion.Name = "cbTargetSdkVersion";
            this.cbTargetSdkVersion.Size = new System.Drawing.Size(350, 21);
            this.cbTargetSdkVersion.TabIndex = 14;
            this.cbTargetSdkVersion.SelectedValueChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // lbTargetSdkVersion
            // 
            this.lbTargetSdkVersion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbTargetSdkVersion.AutoSize = true;
            this.lbTargetSdkVersion.Location = new System.Drawing.Point(3, 192);
            this.lbTargetSdkVersion.Name = "lbTargetSdkVersion";
            this.lbTargetSdkVersion.Size = new System.Drawing.Size(138, 13);
            this.lbTargetSdkVersion.TabIndex = 13;
            this.lbTargetSdkVersion.Text = "Android target SDK version:";
            // 
            // additionalLibrariesControl
            // 
            this.tlpMain.SetColumnSpan(this.additionalLibrariesControl, 3);
            this.additionalLibrariesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.additionalLibrariesControl.Location = new System.Drawing.Point(147, 370);
            this.additionalLibrariesControl.Name = "additionalLibrariesControl";
            this.additionalLibrariesControl.Size = new System.Drawing.Size(350, 94);
            this.additionalLibrariesControl.TabIndex = 41;
            this.additionalLibrariesControl.CheckedLibrariesChanged += new System.EventHandler(this.OnValueChanged);
            // 
            // AndroidPropertyNameControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMain);
            this.MaximumSize = new System.Drawing.Size(500, 543);
            this.MinimumSize = new System.Drawing.Size(500, 543);
            this.Name = "AndroidPropertyNameControl";
            this.Size = new System.Drawing.Size(500, 543);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPackageName;
        private System.Windows.Forms.Label lbApkFilename;
        private System.Windows.Forms.TextBox tbApkFilename;
        private System.Windows.Forms.Label lbFramework;
        private System.Windows.Forms.ComboBox cbAndroidVersion;
        private System.Windows.Forms.Label lbCertificate;
        private System.Windows.Forms.TextBox tbCertificate;
        private System.Windows.Forms.Button cmdBrowseCertificate;
        private System.Windows.Forms.Button cmdNewCertificate;
        private System.Windows.Forms.ComboBox cbTargetSdkVersion;
        private System.Windows.Forms.Label lbTargetSdkVersion;
        private System.Windows.Forms.Label lbAndroidVersionInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbGenerateWcfProxy;
        private System.Windows.Forms.CheckBox cbGenerateWcfProxy;
        private System.Windows.Forms.Label label3;
        private Ide.WizardForms.AdditionalLibrariesControl additionalLibrariesControl;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbRootNamespace;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbAssemblyName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox cbGenerateSetNextInstructionCode;
        private System.Windows.Forms.Label labelSetNextInstructionHelp;
    }
}

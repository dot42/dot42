using Dot42.DeviceLib.UI;

namespace Dot42.Debugger.Forms
{
    partial class MainForm
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
        /// the Types of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.cmdBlackBerryUninstall = new System.Windows.Forms.Button();
            this.cbLaunchAfterInstall = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbDevicePassword = new System.Windows.Forms.TextBox();
            this.cmdBlackBerryInstall = new System.Windows.Forms.Button();
            this.tbDeviceIp = new System.Windows.Forms.TextBox();
            this.cmdBlackberryLogin = new System.Windows.Forms.Button();
            this.cmdMethods = new System.Windows.Forms.Button();
            this.cmdVariableTable = new System.Windows.Forms.Button();
            this.cmdLocals = new System.Windows.Forms.Button();
            this.cmdClassBySignature = new System.Windows.Forms.Button();
            this.tbEntry = new System.Windows.Forms.TextBox();
            this.cmdAllClasses = new System.Windows.Forms.Button();
            this.cmdCallStack = new System.Windows.Forms.Button();
            this.cmdEnableThreadStatus = new System.Windows.Forms.Button();
            this.cmdHelo = new System.Windows.Forms.Button();
            this.cmdResume = new System.Windows.Forms.Button();
            this.cmdSuspend = new System.Windows.Forms.Button();
            this.cmdExit = new System.Windows.Forms.Button();
            this.lbStatus = new System.Windows.Forms.Label();
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miAttach = new System.Windows.Forms.ToolStripMenuItem();
            this.topContainer = new System.Windows.Forms.SplitContainer();
            this.lvDevices = new Dot42.DeviceLib.UI.DevicesListView();
            this.lvProcesses = new Dot42.DeviceLib.UI.JdwpProcessListView();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeView)).BeginInit();
            this.menuBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topContainer)).BeginInit();
            this.topContainer.Panel1.SuspendLayout();
            this.topContainer.Panel2.SuspendLayout();
            this.topContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainContainer
            // 
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(0, 136);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.Controls.Add(this.treeView);
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.Controls.Add(this.cmdBlackBerryUninstall);
            this.mainContainer.Panel2.Controls.Add(this.cbLaunchAfterInstall);
            this.mainContainer.Panel2.Controls.Add(this.label2);
            this.mainContainer.Panel2.Controls.Add(this.label1);
            this.mainContainer.Panel2.Controls.Add(this.tbDevicePassword);
            this.mainContainer.Panel2.Controls.Add(this.cmdBlackBerryInstall);
            this.mainContainer.Panel2.Controls.Add(this.tbDeviceIp);
            this.mainContainer.Panel2.Controls.Add(this.cmdBlackberryLogin);
            this.mainContainer.Panel2.Controls.Add(this.cmdMethods);
            this.mainContainer.Panel2.Controls.Add(this.cmdVariableTable);
            this.mainContainer.Panel2.Controls.Add(this.cmdLocals);
            this.mainContainer.Panel2.Controls.Add(this.cmdClassBySignature);
            this.mainContainer.Panel2.Controls.Add(this.tbEntry);
            this.mainContainer.Panel2.Controls.Add(this.cmdAllClasses);
            this.mainContainer.Panel2.Controls.Add(this.cmdCallStack);
            this.mainContainer.Panel2.Controls.Add(this.cmdEnableThreadStatus);
            this.mainContainer.Panel2.Controls.Add(this.cmdHelo);
            this.mainContainer.Panel2.Controls.Add(this.cmdResume);
            this.mainContainer.Panel2.Controls.Add(this.cmdSuspend);
            this.mainContainer.Panel2.Controls.Add(this.cmdExit);
            this.mainContainer.Panel2.Controls.Add(this.lbStatus);
            this.mainContainer.Size = new System.Drawing.Size(956, 392);
            this.mainContainer.SplitterDistance = 318;
            this.mainContainer.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.treeView.AllowDrop = true;
            this.treeView.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.PathSeparator = ";";
            this.treeView.Size = new System.Drawing.Size(318, 392);
            this.treeView.TabIndex = 0;
            this.treeView.Text = "advTree1";
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterNodeSelect);
            // 
            // cmdBlackBerryUninstall
            // 
            this.cmdBlackBerryUninstall.Location = new System.Drawing.Point(288, 208);
            this.cmdBlackBerryUninstall.Name = "cmdBlackBerryUninstall";
            this.cmdBlackBerryUninstall.Size = new System.Drawing.Size(96, 24);
            this.cmdBlackBerryUninstall.TabIndex = 21;
            this.cmdBlackBerryUninstall.Text = "BB Uninstall";
            this.cmdBlackBerryUninstall.UseVisualStyleBackColor = true;
            this.cmdBlackBerryUninstall.Click += new System.EventHandler(this.cmdBlackBerryUninstall_Click);
            // 
            // cbLaunchAfterInstall
            // 
            this.cbLaunchAfterInstall.AutoSize = true;
            this.cbLaunchAfterInstall.Location = new System.Drawing.Point(112, 296);
            this.cbLaunchAfterInstall.Name = "cbLaunchAfterInstall";
            this.cbLaunchAfterInstall.Size = new System.Drawing.Size(115, 17);
            this.cbLaunchAfterInstall.TabIndex = 20;
            this.cbLaunchAfterInstall.Text = "Launch after install";
            this.cbLaunchAfterInstall.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 264);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Device password:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 240);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Device IP:";
            // 
            // tbDevicePassword
            // 
            this.tbDevicePassword.Location = new System.Drawing.Point(112, 264);
            this.tbDevicePassword.Name = "tbDevicePassword";
            this.tbDevicePassword.Size = new System.Drawing.Size(232, 20);
            this.tbDevicePassword.TabIndex = 17;
            this.tbDevicePassword.Text = "dot42";
            // 
            // cmdBlackBerryInstall
            // 
            this.cmdBlackBerryInstall.Location = new System.Drawing.Point(168, 208);
            this.cmdBlackBerryInstall.Name = "cmdBlackBerryInstall";
            this.cmdBlackBerryInstall.Size = new System.Drawing.Size(96, 24);
            this.cmdBlackBerryInstall.TabIndex = 16;
            this.cmdBlackBerryInstall.Text = "BB Install";
            this.cmdBlackBerryInstall.UseVisualStyleBackColor = true;
            this.cmdBlackBerryInstall.Click += new System.EventHandler(this.cmdBlackBerryInstall_Click);
            // 
            // tbDeviceIp
            // 
            this.tbDeviceIp.Location = new System.Drawing.Point(112, 240);
            this.tbDeviceIp.Name = "tbDeviceIp";
            this.tbDeviceIp.Size = new System.Drawing.Size(232, 20);
            this.tbDeviceIp.TabIndex = 15;
            this.tbDeviceIp.Text = "192.168.140.101";
            // 
            // cmdBlackberryLogin
            // 
            this.cmdBlackberryLogin.Location = new System.Drawing.Point(32, 208);
            this.cmdBlackberryLogin.Name = "cmdBlackberryLogin";
            this.cmdBlackberryLogin.Size = new System.Drawing.Size(112, 24);
            this.cmdBlackberryLogin.TabIndex = 14;
            this.cmdBlackberryLogin.Text = "BB &Login";
            this.cmdBlackberryLogin.UseVisualStyleBackColor = true;
            this.cmdBlackberryLogin.Click += new System.EventHandler(this.cmdBlackberryLogin_Click);
            // 
            // cmdMethods
            // 
            this.cmdMethods.Location = new System.Drawing.Point(240, 56);
            this.cmdMethods.Name = "cmdMethods";
            this.cmdMethods.Size = new System.Drawing.Size(64, 32);
            this.cmdMethods.TabIndex = 13;
            this.cmdMethods.Text = "Method";
            this.cmdMethods.UseVisualStyleBackColor = true;
            this.cmdMethods.Click += new System.EventHandler(this.cmdMethods_Click);
            // 
            // cmdVariableTable
            // 
            this.cmdVariableTable.Location = new System.Drawing.Point(312, 56);
            this.cmdVariableTable.Name = "cmdVariableTable";
            this.cmdVariableTable.Size = new System.Drawing.Size(64, 32);
            this.cmdVariableTable.TabIndex = 12;
            this.cmdVariableTable.Text = "Var-Table";
            this.cmdVariableTable.UseVisualStyleBackColor = true;
            this.cmdVariableTable.Click += new System.EventHandler(this.getVariableTable_Click);
            // 
            // cmdLocals
            // 
            this.cmdLocals.Location = new System.Drawing.Point(96, 96);
            this.cmdLocals.Name = "cmdLocals";
            this.cmdLocals.Size = new System.Drawing.Size(64, 32);
            this.cmdLocals.TabIndex = 11;
            this.cmdLocals.Text = "&Locals";
            this.cmdLocals.UseVisualStyleBackColor = true;
            this.cmdLocals.Click += new System.EventHandler(this.cmdLocals_Click);
            // 
            // cmdClassBySignature
            // 
            this.cmdClassBySignature.Location = new System.Drawing.Point(360, 16);
            this.cmdClassBySignature.Name = "cmdClassBySignature";
            this.cmdClassBySignature.Size = new System.Drawing.Size(115, 32);
            this.cmdClassBySignature.TabIndex = 10;
            this.cmdClassBySignature.Text = "ClassWithSignature";
            this.cmdClassBySignature.UseVisualStyleBackColor = true;
            this.cmdClassBySignature.Click += new System.EventHandler(this.cmdClassBySignature_Click);
            // 
            // tbEntry
            // 
            this.tbEntry.Location = new System.Drawing.Point(48, 152);
            this.tbEntry.Name = "tbEntry";
            this.tbEntry.Size = new System.Drawing.Size(272, 20);
            this.tbEntry.TabIndex = 9;
            // 
            // cmdAllClasses
            // 
            this.cmdAllClasses.Location = new System.Drawing.Point(168, 56);
            this.cmdAllClasses.Name = "cmdAllClasses";
            this.cmdAllClasses.Size = new System.Drawing.Size(64, 32);
            this.cmdAllClasses.TabIndex = 8;
            this.cmdAllClasses.Text = "AllClasses";
            this.cmdAllClasses.UseVisualStyleBackColor = true;
            this.cmdAllClasses.Click += new System.EventHandler(this.cmdAllClasses_Click);
            // 
            // cmdCallStack
            // 
            this.cmdCallStack.Location = new System.Drawing.Point(96, 56);
            this.cmdCallStack.Name = "cmdCallStack";
            this.cmdCallStack.Size = new System.Drawing.Size(64, 32);
            this.cmdCallStack.TabIndex = 7;
            this.cmdCallStack.Text = "CallStack";
            this.cmdCallStack.UseVisualStyleBackColor = true;
            this.cmdCallStack.Click += new System.EventHandler(this.cmdCallStack_Click);
            // 
            // cmdEnableThreadStatus
            // 
            this.cmdEnableThreadStatus.Location = new System.Drawing.Point(496, 48);
            this.cmdEnableThreadStatus.Name = "cmdEnableThreadStatus";
            this.cmdEnableThreadStatus.Size = new System.Drawing.Size(64, 24);
            this.cmdEnableThreadStatus.TabIndex = 6;
            this.cmdEnableThreadStatus.Text = "THST";
            this.cmdEnableThreadStatus.UseVisualStyleBackColor = true;
            this.cmdEnableThreadStatus.Click += new System.EventHandler(this.cmdEnableThreadStatus_Click);
            // 
            // cmdHelo
            // 
            this.cmdHelo.Location = new System.Drawing.Point(496, 16);
            this.cmdHelo.Name = "cmdHelo";
            this.cmdHelo.Size = new System.Drawing.Size(64, 24);
            this.cmdHelo.TabIndex = 5;
            this.cmdHelo.Text = "HELO";
            this.cmdHelo.UseVisualStyleBackColor = true;
            this.cmdHelo.Click += new System.EventHandler(this.cmdHelo_Click);
            // 
            // cmdResume
            // 
            this.cmdResume.Location = new System.Drawing.Point(96, 16);
            this.cmdResume.Name = "cmdResume";
            this.cmdResume.Size = new System.Drawing.Size(64, 32);
            this.cmdResume.TabIndex = 4;
            this.cmdResume.Text = "Resume";
            this.cmdResume.UseVisualStyleBackColor = true;
            this.cmdResume.Click += new System.EventHandler(this.cmdResume_Click);
            // 
            // cmdSuspend
            // 
            this.cmdSuspend.Location = new System.Drawing.Point(24, 16);
            this.cmdSuspend.Name = "cmdSuspend";
            this.cmdSuspend.Size = new System.Drawing.Size(64, 32);
            this.cmdSuspend.TabIndex = 3;
            this.cmdSuspend.Text = "Suspend";
            this.cmdSuspend.UseVisualStyleBackColor = true;
            this.cmdSuspend.Click += new System.EventHandler(this.cmdSuspend_Click);
            // 
            // cmdExit
            // 
            this.cmdExit.Location = new System.Drawing.Point(168, 16);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.Size = new System.Drawing.Size(64, 32);
            this.cmdExit.TabIndex = 1;
            this.cmdExit.Text = "Exit";
            this.cmdExit.UseVisualStyleBackColor = true;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbStatus.Location = new System.Drawing.Point(0, 0);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(13, 13);
            this.lbStatus.TabIndex = 0;
            this.lbStatus.Text = "?";
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(956, 24);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "menuStrip1";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAttach});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(37, 20);
            this.miFile.Text = "File";
            // 
            // miAttach
            // 
            this.miAttach.Name = "miAttach";
            this.miAttach.Size = new System.Drawing.Size(109, 22);
            this.miAttach.Text = "Attach";
            this.miAttach.Click += new System.EventHandler(this.OnAttachClick);
            // 
            // topContainer
            // 
            this.topContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.topContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.topContainer.Location = new System.Drawing.Point(0, 24);
            this.topContainer.Name = "topContainer";
            // 
            // topContainer.Panel1
            // 
            this.topContainer.Panel1.Controls.Add(this.lvDevices);
            // 
            // topContainer.Panel2
            // 
            this.topContainer.Panel2.Controls.Add(this.lvProcesses);
            this.topContainer.Size = new System.Drawing.Size(956, 112);
            this.topContainer.SplitterDistance = 583;
            this.topContainer.TabIndex = 3;
            // 
            // lvDevices
            // 
            this.lvDevices.DeviceMonitorEnabled = false;
            this.lvDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDevices.IsCompatibleCheck = null;
            this.lvDevices.Location = new System.Drawing.Point(0, 0);
            this.lvDevices.Name = "lvDevices";
            this.lvDevices.Size = new System.Drawing.Size(583, 112);
            this.lvDevices.TabIndex = 3;
            this.lvDevices.SelectedDeviceChanged += new System.EventHandler(this.OnSelectedDeviceChanged);
            // 
            // lvProcesses
            // 
            this.lvProcesses.Device = null;
            this.lvProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProcesses.Location = new System.Drawing.Point(0, 0);
            this.lvProcesses.Name = "lvProcesses";
            this.lvProcesses.Size = new System.Drawing.Size(369, 112);
            this.lvProcesses.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 528);
            this.Controls.Add(this.mainContainer);
            this.Controls.Add(this.topContainer);
            this.Controls.Add(this.menuBar);
            this.MainMenuStrip = this.menuBar;
            this.Name = "MainForm";
            this.Text = "Dot42 Debugger";
            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);
            this.mainContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
            this.mainContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeView)).EndInit();
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.topContainer.Panel1.ResumeLayout(false);
            this.topContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.topContainer)).EndInit();
            this.topContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripMenuItem miAttach;
        private System.Windows.Forms.SplitContainer topContainer;
        private DevicesListView lvDevices;
        private JdwpProcessListView lvProcesses;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Button cmdExit;
        private System.Windows.Forms.Button cmdResume;
        private System.Windows.Forms.Button cmdSuspend;
        private System.Windows.Forms.Button cmdHelo;
        private System.Windows.Forms.Button cmdEnableThreadStatus;
        private System.Windows.Forms.Button cmdCallStack;
        private System.Windows.Forms.Button cmdAllClasses;
        private System.Windows.Forms.Button cmdClassBySignature;
        private System.Windows.Forms.TextBox tbEntry;
        private System.Windows.Forms.Button cmdLocals;
        private System.Windows.Forms.Button cmdVariableTable;
        private System.Windows.Forms.Button cmdMethods;
        private System.Windows.Forms.TextBox tbDeviceIp;
        private System.Windows.Forms.Button cmdBlackberryLogin;
        private System.Windows.Forms.Button cmdBlackBerryInstall;
        private System.Windows.Forms.TextBox tbDevicePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbLaunchAfterInstall;
        private System.Windows.Forms.Button cmdBlackBerryUninstall;
    }
}
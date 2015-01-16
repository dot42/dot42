using System.ComponentModel;
using DevComponents.DotNetBar;
using Dot42.Gui.Controls.Devices;
using Dot42.Shared.UI;

namespace Dot42.Gui.Forms.BlackBerry
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.styleManager1 = new DevComponents.DotNetBar.StyleManager(this.components);
            this.ribbon = new DevComponents.DotNetBar.RibbonControl();
            this.ribbonPanelDevices = new DevComponents.DotNetBar.RibbonPanel();
            this.rbEmulators = new DevComponents.DotNetBar.RibbonBar();
            this.rbDeviceConnect = new DevComponents.DotNetBar.RibbonBar();
            this.buttonAddDevice = new DevComponents.DotNetBar.ButtonItem();
            this.rbDevicesTools = new DevComponents.DotNetBar.RibbonBar();
            this.buttonInstallApk = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStartActivity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonLogCat = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanelHelp = new DevComponents.DotNetBar.RibbonPanel();
            this.rbHelpProduct = new DevComponents.DotNetBar.RibbonBar();
            this.buttonCheckForUpdates = new DevComponents.DotNetBar.ButtonItem();
            this.buttonActivate = new DevComponents.DotNetBar.ButtonItem();
            this.rbHelpLinks = new DevComponents.DotNetBar.RibbonBar();
            this.buttonWebsite = new DevComponents.DotNetBar.ButtonItem();
            this.rbHelpInfo = new DevComponents.DotNetBar.RibbonBar();
            this.buttonOpenSamples = new DevComponents.DotNetBar.ButtonItem();
            this.tabItemDevices = new DevComponents.DotNetBar.RibbonTabItem();
            this.tabItemHelp = new DevComponents.DotNetBar.RibbonTabItem();
            this.statusBar = new DevComponents.DotNetBar.Bar();
            this.lbVersion = new DevComponents.DotNetBar.LabelItem();
            this.lbLicStatus = new DevComponents.DotNetBar.LabelItem();
            this.modalPanelContainer1 = new Dot42.Shared.UI.ModalPanelContainer();
            this.devicesControl = new Dot42.Gui.Controls.Devices.DevicesControl();
            this.buttonRemoveDevice = new DevComponents.DotNetBar.ButtonItem();
            this.ribbon.SuspendLayout();
            this.ribbonPanelDevices.SuspendLayout();
            this.ribbonPanelHelp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statusBar)).BeginInit();
            this.modalPanelContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // styleManager1
            // 
            this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2010Blue;
            this.styleManager1.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(205)))), ((int)(((byte)(209))))), System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(9)))), ((int)(((byte)(10))))));
            // 
            // ribbon
            // 
            this.ribbon.AutoExpand = false;
            this.ribbon.AutoKeyboardExpand = false;
            this.ribbon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(222)))), ((int)(((byte)(192)))));
            // 
            // 
            // 
            this.ribbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbon.CanCustomize = false;
            this.ribbon.CaptionVisible = true;
            this.ribbon.Controls.Add(this.ribbonPanelDevices);
            this.ribbon.Controls.Add(this.ribbonPanelHelp);
            this.ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbon.ForeColor = System.Drawing.Color.Black;
            this.ribbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.tabItemDevices,
            this.tabItemHelp});
            this.ribbon.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.ribbon.Location = new System.Drawing.Point(5, 1);
            this.ribbon.Name = "ribbon";
            this.ribbon.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ribbon.Size = new System.Drawing.Size(1113, 111);
            this.ribbon.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbon.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.ribbon.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.ribbon.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.ribbon.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.ribbon.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.ribbon.SystemText.QatDialogAddButton = "&Add >>";
            this.ribbon.SystemText.QatDialogCancelButton = "Cancel";
            this.ribbon.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.ribbon.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.ribbon.SystemText.QatDialogOkButton = "OK";
            this.ribbon.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbon.SystemText.QatDialogRemoveButton = "&Remove";
            this.ribbon.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.ribbon.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbon.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            this.ribbon.TabGroupHeight = 14;
            this.ribbon.TabIndex = 0;
            this.ribbon.UseCustomizeDialog = false;
            // 
            // ribbonPanelDevices
            // 
            this.ribbonPanelDevices.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanelDevices.Controls.Add(this.rbEmulators);
            this.ribbonPanelDevices.Controls.Add(this.rbDeviceConnect);
            this.ribbonPanelDevices.Controls.Add(this.rbDevicesTools);
            this.ribbonPanelDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanelDevices.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanelDevices.Name = "ribbonPanelDevices";
            this.ribbonPanelDevices.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanelDevices.Size = new System.Drawing.Size(1113, 55);
            // 
            // 
            // 
            this.ribbonPanelDevices.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelDevices.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelDevices.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanelDevices.TabIndex = 5;
            // 
            // rbEmulators
            // 
            this.rbEmulators.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbEmulators.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbEmulators.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbEmulators.ContainerControlProcessDialogKey = true;
            this.rbEmulators.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbEmulators.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbEmulators.Location = new System.Drawing.Point(605, 0);
            this.rbEmulators.Name = "rbEmulators";
            this.rbEmulators.Size = new System.Drawing.Size(693, 52);
            this.rbEmulators.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbEmulators.TabIndex = 2;
            // 
            // 
            // 
            this.rbEmulators.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbEmulators.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbEmulators.TitleVisible = false;
            // 
            // rbDeviceConnect
            // 
            this.rbDeviceConnect.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbDeviceConnect.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbDeviceConnect.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbDeviceConnect.ContainerControlProcessDialogKey = true;
            this.rbDeviceConnect.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbDeviceConnect.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonAddDevice,
            this.buttonRemoveDevice});
            this.rbDeviceConnect.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbDeviceConnect.Location = new System.Drawing.Point(329, 0);
            this.rbDeviceConnect.Name = "rbDeviceConnect";
            this.rbDeviceConnect.Size = new System.Drawing.Size(276, 52);
            this.rbDeviceConnect.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbDeviceConnect.TabIndex = 1;
            // 
            // 
            // 
            this.rbDeviceConnect.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbDeviceConnect.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbDeviceConnect.TitleVisible = false;
            // 
            // buttonAddDevice
            // 
            this.buttonAddDevice.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonAddDevice.Name = "buttonAddDevice";
            this.buttonAddDevice.SubItemsExpandWidth = 14;
            this.buttonAddDevice.Text = "Add device";
            // 
            // rbDevicesTools
            // 
            this.rbDevicesTools.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbDevicesTools.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbDevicesTools.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbDevicesTools.ContainerControlProcessDialogKey = true;
            this.rbDevicesTools.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbDevicesTools.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonInstallApk,
            this.buttonStartActivity,
            this.buttonLogCat});
            this.rbDevicesTools.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbDevicesTools.Location = new System.Drawing.Point(3, 0);
            this.rbDevicesTools.Name = "rbDevicesTools";
            this.rbDevicesTools.Size = new System.Drawing.Size(326, 52);
            this.rbDevicesTools.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbDevicesTools.TabIndex = 0;
            // 
            // 
            // 
            this.rbDevicesTools.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbDevicesTools.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbDevicesTools.TitleVisible = false;
            // 
            // buttonInstallApk
            // 
            this.buttonInstallApk.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonInstallApk.Enabled = false;
            this.buttonInstallApk.Name = "buttonInstallApk";
            this.buttonInstallApk.SubItemsExpandWidth = 14;
            this.buttonInstallApk.Text = "Install BAR";
            // 
            // buttonStartActivity
            // 
            this.buttonStartActivity.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonStartActivity.Enabled = false;
            this.buttonStartActivity.Name = "buttonStartActivity";
            this.buttonStartActivity.SubItemsExpandWidth = 14;
            this.buttonStartActivity.Text = "Start activity";
            this.buttonStartActivity.Visible = false;
            // 
            // buttonLogCat
            // 
            this.buttonLogCat.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonLogCat.Enabled = false;
            this.buttonLogCat.Name = "buttonLogCat";
            this.buttonLogCat.SubItemsExpandWidth = 14;
            this.buttonLogCat.Text = "Device log";
            // 
            // ribbonPanelHelp
            // 
            this.ribbonPanelHelp.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanelHelp.Controls.Add(this.rbHelpProduct);
            this.ribbonPanelHelp.Controls.Add(this.rbHelpLinks);
            this.ribbonPanelHelp.Controls.Add(this.rbHelpInfo);
            this.ribbonPanelHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanelHelp.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanelHelp.Name = "ribbonPanelHelp";
            this.ribbonPanelHelp.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanelHelp.Size = new System.Drawing.Size(1113, 55);
            // 
            // 
            // 
            this.ribbonPanelHelp.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelHelp.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelHelp.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanelHelp.TabIndex = 0;
            this.ribbonPanelHelp.Visible = false;
            // 
            // rbHelpProduct
            // 
            this.rbHelpProduct.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbHelpProduct.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpProduct.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpProduct.ContainerControlProcessDialogKey = true;
            this.rbHelpProduct.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbHelpProduct.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonCheckForUpdates,
            this.buttonActivate});
            this.rbHelpProduct.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbHelpProduct.Location = new System.Drawing.Point(153, 0);
            this.rbHelpProduct.Name = "rbHelpProduct";
            this.rbHelpProduct.Size = new System.Drawing.Size(108, 52);
            this.rbHelpProduct.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbHelpProduct.TabIndex = 3;
            // 
            // 
            // 
            this.rbHelpProduct.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpProduct.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpProduct.TitleVisible = false;
            // 
            // buttonCheckForUpdates
            // 
            this.buttonCheckForUpdates.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonCheckForUpdates.Name = "buttonCheckForUpdates";
            this.buttonCheckForUpdates.SubItemsExpandWidth = 14;
            this.buttonCheckForUpdates.Text = "Check for<br/>updates";
            this.buttonCheckForUpdates.Click += new System.EventHandler(this.OnCheckForUpdatesClick);
            // 
            // buttonActivate
            // 
            this.buttonActivate.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonActivate.Name = "buttonActivate";
            this.buttonActivate.SubItemsExpandWidth = 14;
            this.buttonActivate.Text = "Activate";
            this.buttonActivate.Click += new System.EventHandler(this.OnActivateClick);
            // 
            // rbHelpLinks
            // 
            this.rbHelpLinks.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbHelpLinks.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpLinks.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpLinks.ContainerControlProcessDialogKey = true;
            this.rbHelpLinks.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbHelpLinks.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonWebsite});
            this.rbHelpLinks.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbHelpLinks.Location = new System.Drawing.Point(88, 0);
            this.rbHelpLinks.Name = "rbHelpLinks";
            this.rbHelpLinks.Size = new System.Drawing.Size(65, 52);
            this.rbHelpLinks.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbHelpLinks.TabIndex = 2;
            // 
            // 
            // 
            this.rbHelpLinks.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpLinks.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpLinks.TitleVisible = false;
            // 
            // buttonWebsite
            // 
            this.buttonWebsite.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonWebsite.Name = "buttonWebsite";
            this.buttonWebsite.SubItemsExpandWidth = 14;
            this.buttonWebsite.Text = "dot42.com";
            this.buttonWebsite.Click += new System.EventHandler(this.OnOpenWebsite);
            // 
            // rbHelpInfo
            // 
            this.rbHelpInfo.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.rbHelpInfo.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpInfo.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpInfo.ContainerControlProcessDialogKey = true;
            this.rbHelpInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbHelpInfo.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonOpenSamples});
            this.rbHelpInfo.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.rbHelpInfo.Location = new System.Drawing.Point(3, 0);
            this.rbHelpInfo.Name = "rbHelpInfo";
            this.rbHelpInfo.Size = new System.Drawing.Size(85, 52);
            this.rbHelpInfo.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.rbHelpInfo.TabIndex = 0;
            // 
            // 
            // 
            this.rbHelpInfo.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.rbHelpInfo.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbHelpInfo.TitleVisible = false;
            // 
            // buttonOpenSamples
            // 
            this.buttonOpenSamples.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonOpenSamples.Name = "buttonOpenSamples";
            this.buttonOpenSamples.SubItemsExpandWidth = 14;
            this.buttonOpenSamples.Text = "Open samples<br/>folder";
            this.buttonOpenSamples.Click += new System.EventHandler(this.OnOpenSampleFolderExecuted);
            // 
            // tabItemDevices
            // 
            this.tabItemDevices.Checked = true;
            this.tabItemDevices.Name = "tabItemDevices";
            this.tabItemDevices.Panel = this.ribbonPanelDevices;
            this.tabItemDevices.Text = "&DEVICES";
            // 
            // tabItemHelp
            // 
            this.tabItemHelp.Name = "tabItemHelp";
            this.tabItemHelp.Panel = this.ribbonPanelHelp;
            this.tabItemHelp.Text = "HELP";
            // 
            // statusBar
            // 
            this.statusBar.AccessibleDescription = "DotNetBar Bar (statusBar)";
            this.statusBar.AccessibleName = "DotNetBar Bar";
            this.statusBar.AccessibleRole = System.Windows.Forms.AccessibleRole.StatusBar;
            this.statusBar.BarType = DevComponents.DotNetBar.eBarType.StatusBar;
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBar.GrabHandleStyle = DevComponents.DotNetBar.eGrabHandleStyle.ResizeHandle;
            this.statusBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.lbVersion,
            this.lbLicStatus});
            this.statusBar.Location = new System.Drawing.Point(5, 513);
            this.statusBar.Name = "statusBar";
            this.statusBar.PaddingBottom = 2;
            this.statusBar.PaddingLeft = 3;
            this.statusBar.PaddingRight = 3;
            this.statusBar.PaddingTop = 2;
            this.statusBar.Size = new System.Drawing.Size(1113, 19);
            this.statusBar.Stretch = true;
            this.statusBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.statusBar.TabIndex = 8;
            this.statusBar.TabStop = false;
            // 
            // lbVersion
            // 
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Stretch = true;
            this.lbVersion.Text = "A";
            // 
            // lbLicStatus
            // 
            this.lbLicStatus.Name = "lbLicStatus";
            this.lbLicStatus.Text = "B";
            // 
            // modalPanelContainer1
            // 
            this.modalPanelContainer1.Controls.Add(this.devicesControl);
            this.modalPanelContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modalPanelContainer1.Location = new System.Drawing.Point(5, 112);
            this.modalPanelContainer1.Name = "modalPanelContainer1";
            this.modalPanelContainer1.Size = new System.Drawing.Size(1113, 401);
            this.modalPanelContainer1.TabIndex = 4;
            // 
            // devicesControl
            // 
            this.devicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesControl.Location = new System.Drawing.Point(0, 0);
            this.devicesControl.Name = "devicesControl";
            this.devicesControl.Padding = new System.Windows.Forms.Padding(8);
            this.devicesControl.Size = new System.Drawing.Size(1113, 401);
            this.devicesControl.TabIndex = 2;
            this.devicesControl.Text = "devicesControl1";
            this.devicesControl.UsesBlockingAnimation = false;
            // 
            // buttonRemoveDevice
            // 
            this.buttonRemoveDevice.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonRemoveDevice.Name = "buttonRemoveBlackBerryDevice";
            this.buttonRemoveDevice.SubItemsExpandWidth = 14;
            this.buttonRemoveDevice.Text = "Remove device";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1123, 534);
            this.Controls.Add(this.modalPanelContainer1);
            this.Controls.Add(this.ribbon);
            this.Controls.Add(this.statusBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "dot42 C# for BlackBerry Device Center";
            this.ribbon.ResumeLayout(false);
            this.ribbon.PerformLayout();
            this.ribbonPanelDevices.ResumeLayout(false);
            this.ribbonPanelHelp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statusBar)).EndInit();
            this.modalPanelContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private StyleManager styleManager1;
        private RibbonControl ribbon;
        private RibbonTabItem tabItemDevices;
        private DevicesControl devicesControl;
        private RibbonBar rbDevicesTools;
        private ButtonItem buttonInstallApk;
        private ButtonItem buttonStartActivity;
        private ModalPanelContainer modalPanelContainer1;
        private RibbonBar rbDeviceConnect;
        private ButtonItem buttonLogCat;
        private RibbonPanel ribbonPanelDevices;
        private RibbonBar rbHelpInfo;
        private ButtonItem buttonOpenSamples;
        private RibbonBar rbEmulators;
        private RibbonBar rbHelpLinks;
        private RibbonPanel ribbonPanelHelp;
        private RibbonTabItem tabItemHelp;
        private Bar statusBar;
        private LabelItem lbVersion;
        private LabelItem lbLicStatus;
        private RibbonBar rbHelpProduct;
        private ButtonItem buttonCheckForUpdates;
        private ButtonItem buttonActivate;
        private ButtonItem buttonWebsite;
        private ButtonItem buttonAddDevice;
        private ButtonItem buttonRemoveDevice;
    }
}
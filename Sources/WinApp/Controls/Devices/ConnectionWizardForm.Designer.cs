using DevComponents.DotNetBar.Controls;

namespace Dot42.Gui.Controls.Devices
{
    partial class ConnectionWizardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionWizardForm));
            this.wizard = new DevComponents.DotNetBar.Wizard();
            this.welcomePage = new DevComponents.DotNetBar.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.connectOverWifiPage = new DevComponents.DotNetBar.WizardPage();
            this.lbWifiOtherOptions = new DevComponents.DotNetBar.LabelX();
            this.cmdConnectOverWifi = new DevComponents.DotNetBar.ButtonX();
            this.switchButtonAdbWifiInstalled = new DevComponents.DotNetBar.Controls.SwitchButton();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.lbWifiResult = new DevComponents.DotNetBar.LabelX();
            this.switchButtonIsRooted = new DevComponents.DotNetBar.Controls.SwitchButton();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.switchButtonWifi = new DevComponents.DotNetBar.Controls.SwitchButton();
            this.enableUsbDebuggingPage = new DevComponents.DotNetBar.WizardPage();
            this.labelX10 = new DevComponents.DotNetBar.LabelX();
            this.manufacturerSelectionPage = new DevComponents.DotNetBar.WizardPage();
            this.cbManufacturer = new DevComponents.DotNetBar.Controls.ListViewEx();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.manufacturerInfoPage = new DevComponents.DotNetBar.WizardPage();
            this.labelX9 = new DevComponents.DotNetBar.LabelX();
            this.lbManufacturerHelp = new DevComponents.DotNetBar.LabelX();
            this.cannotConnectPage = new DevComponents.DotNetBar.WizardPage();
            this.sendToSupportProgress = new DevComponents.DotNetBar.Controls.CircularProgress();
            this.cmdSendToSupport = new DevComponents.DotNetBar.ButtonX();
            this.tbDevice = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelX8 = new DevComponents.DotNetBar.LabelX();
            this.labelX7 = new DevComponents.DotNetBar.LabelX();
            this.tbEmail = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.tbName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelX6 = new DevComponents.DotNetBar.LabelX();
            this.labelX5 = new DevComponents.DotNetBar.LabelX();
            this.connectionTypePage = new DevComponents.DotNetBar.WizardPage();
            this.labelX11 = new DevComponents.DotNetBar.LabelX();
            this.cbConnectViaUsb = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.cbConnectViaWifi = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.wizard.SuspendLayout();
            this.welcomePage.SuspendLayout();
            this.connectOverWifiPage.SuspendLayout();
            this.enableUsbDebuggingPage.SuspendLayout();
            this.manufacturerSelectionPage.SuspendLayout();
            this.manufacturerInfoPage.SuspendLayout();
            this.cannotConnectPage.SuspendLayout();
            this.connectionTypePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard
            // 
            this.wizard.CancelButtonText = "Cancel";
            this.wizard.Cursor = System.Windows.Forms.Cursors.Default;
            this.wizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard.FinishButtonTabIndex = 3;
            // 
            // 
            // 
            this.wizard.FooterStyle.BackColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BackColorGradientAngle = 90;
            this.wizard.FooterStyle.BorderBottomWidth = 1;
            this.wizard.FooterStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BorderLeftWidth = 1;
            this.wizard.FooterStyle.BorderRightWidth = 1;
            this.wizard.FooterStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Etched;
            this.wizard.FooterStyle.BorderTopColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BorderTopWidth = 1;
            this.wizard.FooterStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.wizard.FooterStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.wizard.FooterStyle.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.wizard.HeaderCaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard.HeaderDescriptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard.HeaderDescriptionIndent = 16;
            // 
            // 
            // 
            this.wizard.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.wizard.HeaderStyle.BackColorGradientAngle = 90;
            this.wizard.HeaderStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Etched;
            this.wizard.HeaderStyle.BorderBottomWidth = 1;
            this.wizard.HeaderStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.wizard.HeaderStyle.BorderLeftWidth = 1;
            this.wizard.HeaderStyle.BorderRightWidth = 1;
            this.wizard.HeaderStyle.BorderTopWidth = 1;
            this.wizard.HeaderStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.wizard.HeaderStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.wizard.HeaderStyle.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.wizard.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.wizard.Location = new System.Drawing.Point(0, 0);
            this.wizard.Name = "wizard";
            this.wizard.Size = new System.Drawing.Size(809, 409);
            this.wizard.TabIndex = 0;
            this.wizard.WizardPages.AddRange(new DevComponents.DotNetBar.WizardPage[] {
            this.welcomePage,
            this.connectionTypePage,
            this.connectOverWifiPage,
            this.enableUsbDebuggingPage,
            this.manufacturerSelectionPage,
            this.manufacturerInfoPage,
            this.cannotConnectPage});
            this.wizard.FinishButtonClick += new System.ComponentModel.CancelEventHandler(this.OnFinishButtonClick);
            this.wizard.WizardPageChanging += new DevComponents.DotNetBar.WizardCancelPageChangeEventHandler(this.OnWizardPageChanging);
            // 
            // welcomePage
            // 
            this.welcomePage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.welcomePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.welcomePage.Controls.Add(this.label1);
            this.welcomePage.Controls.Add(this.label2);
            this.welcomePage.Controls.Add(this.label3);
            this.welcomePage.InteriorPage = false;
            this.welcomePage.Location = new System.Drawing.Point(0, 0);
            this.welcomePage.Name = "welcomePage";
            this.welcomePage.Size = new System.Drawing.Size(809, 363);
            // 
            // 
            // 
            this.welcomePage.Style.BackColor = System.Drawing.Color.White;
            this.welcomePage.Style.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("welcomePage.Style.BackgroundImage")));
            this.welcomePage.Style.BackgroundImagePosition = DevComponents.DotNetBar.eStyleBackgroundImage.TopLeft;
            this.welcomePage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.welcomePage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.welcomePage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.welcomePage.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 16F);
            this.label1.Location = new System.Drawing.Point(210, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(585, 66);
            this.label1.TabIndex = 0;
            this.label1.Text = "Welcome to the Device Connection Wizard";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(210, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(584, 231);
            this.label2.TabIndex = 1;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(210, 340);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "To continue, click Next.";
            // 
            // connectOverWifiPage
            // 
            this.connectOverWifiPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectOverWifiPage.AntiAlias = false;
            this.connectOverWifiPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.connectOverWifiPage.Controls.Add(this.lbWifiOtherOptions);
            this.connectOverWifiPage.Controls.Add(this.cmdConnectOverWifi);
            this.connectOverWifiPage.Controls.Add(this.switchButtonAdbWifiInstalled);
            this.connectOverWifiPage.Controls.Add(this.labelX3);
            this.connectOverWifiPage.Controls.Add(this.lbWifiResult);
            this.connectOverWifiPage.Controls.Add(this.switchButtonIsRooted);
            this.connectOverWifiPage.Controls.Add(this.labelX2);
            this.connectOverWifiPage.Controls.Add(this.labelX1);
            this.connectOverWifiPage.Controls.Add(this.switchButtonWifi);
            this.connectOverWifiPage.Location = new System.Drawing.Point(7, 72);
            this.connectOverWifiPage.Name = "connectOverWifiPage";
            this.connectOverWifiPage.PageDescription = "Let\'s find out if it is possible to connect your device over a WIFI network.";
            this.connectOverWifiPage.PageTitle = "Connect over a WIFI network";
            this.connectOverWifiPage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.connectOverWifiPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.connectOverWifiPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.connectOverWifiPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.connectOverWifiPage.TabIndex = 9;
            this.connectOverWifiPage.AfterPageDisplayed += new DevComponents.DotNetBar.WizardPageChangeEventHandler(this.OnConnectOverWifiPageDisplayed);
            // 
            // lbWifiOtherOptions
            // 
            this.lbWifiOtherOptions.AutoSize = true;
            // 
            // 
            // 
            this.lbWifiOtherOptions.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbWifiOtherOptions.Location = new System.Drawing.Point(112, 256);
            this.lbWifiOtherOptions.Name = "lbWifiOtherOptions";
            this.lbWifiOtherOptions.Size = new System.Drawing.Size(235, 15);
            this.lbWifiOtherOptions.TabIndex = 8;
            this.lbWifiOtherOptions.Text = "Click Next to connect to your device using USB.";
            // 
            // cmdConnectOverWifi
            // 
            this.cmdConnectOverWifi.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdConnectOverWifi.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdConnectOverWifi.Location = new System.Drawing.Point(592, 176);
            this.cmdConnectOverWifi.Name = "cmdConnectOverWifi";
            this.cmdConnectOverWifi.Size = new System.Drawing.Size(120, 48);
            this.cmdConnectOverWifi.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdConnectOverWifi.TabIndex = 7;
            this.cmdConnectOverWifi.Text = "Connect";
            this.cmdConnectOverWifi.Visible = false;
            this.cmdConnectOverWifi.Click += new System.EventHandler(this.OnConnectOverWifiClick);
            // 
            // switchButtonAdbWifiInstalled
            // 
            // 
            // 
            // 
            this.switchButtonAdbWifiInstalled.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.switchButtonAdbWifiInstalled.Location = new System.Drawing.Point(448, 112);
            this.switchButtonAdbWifiInstalled.Name = "switchButtonAdbWifiInstalled";
            this.switchButtonAdbWifiInstalled.OffText = "NO / I DON\'T KNOW";
            this.switchButtonAdbWifiInstalled.OnText = "YES";
            this.switchButtonAdbWifiInstalled.Size = new System.Drawing.Size(158, 24);
            this.switchButtonAdbWifiInstalled.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.switchButtonAdbWifiInstalled.TabIndex = 6;
            this.switchButtonAdbWifiInstalled.ValueChanged += new System.EventHandler(this.OnWifiOptionChanged);
            // 
            // labelX3
            // 
            this.labelX3.AutoSize = true;
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Location = new System.Drawing.Point(112, 120);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(299, 15);
            this.labelX3.TabIndex = 5;
            this.labelX3.Text = "Have your installed ADB Wifi (or similar app) on your device?";
            // 
            // lbWifiResult
            // 
            // 
            // 
            // 
            this.lbWifiResult.BackgroundStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.lbWifiResult.BackgroundStyle.BorderTopColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.lbWifiResult.BackgroundStyle.BorderTopWidth = 1;
            this.lbWifiResult.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbWifiResult.Location = new System.Drawing.Point(112, 160);
            this.lbWifiResult.Name = "lbWifiResult";
            this.lbWifiResult.PaddingTop = 8;
            this.lbWifiResult.Size = new System.Drawing.Size(600, 56);
            this.lbWifiResult.TabIndex = 4;
            this.lbWifiResult.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.lbWifiResult.MarkupLinkClick += new DevComponents.DotNetBar.MarkupLinkClickEventHandler(this.OnMarkupLinkClick);
            // 
            // switchButtonIsRooted
            // 
            // 
            // 
            // 
            this.switchButtonIsRooted.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.switchButtonIsRooted.Location = new System.Drawing.Point(448, 72);
            this.switchButtonIsRooted.Name = "switchButtonIsRooted";
            this.switchButtonIsRooted.OffText = "NO /  I DON\'T KNOW";
            this.switchButtonIsRooted.OnText = "YES";
            this.switchButtonIsRooted.Size = new System.Drawing.Size(158, 24);
            this.switchButtonIsRooted.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.switchButtonIsRooted.TabIndex = 3;
            this.switchButtonIsRooted.ValueChanged += new System.EventHandler(this.OnWifiOptionChanged);
            // 
            // labelX2
            // 
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Location = new System.Drawing.Point(112, 72);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(144, 16);
            this.labelX2.TabIndex = 2;
            this.labelX2.Text = "Is your device rooted?";
            // 
            // labelX1
            // 
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(112, 32);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(288, 16);
            this.labelX1.TabIndex = 1;
            this.labelX1.Text = "Does your device have a working WIFI connection?";
            // 
            // switchButtonWifi
            // 
            // 
            // 
            // 
            this.switchButtonWifi.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.switchButtonWifi.Location = new System.Drawing.Point(448, 32);
            this.switchButtonWifi.Name = "switchButtonWifi";
            this.switchButtonWifi.OffText = "NO";
            this.switchButtonWifi.OnText = "YES";
            this.switchButtonWifi.Size = new System.Drawing.Size(158, 24);
            this.switchButtonWifi.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.switchButtonWifi.TabIndex = 0;
            this.switchButtonWifi.Value = true;
            this.switchButtonWifi.ValueObject = "Y";
            this.switchButtonWifi.ValueChanged += new System.EventHandler(this.OnWifiOptionChanged);
            // 
            // enableUsbDebuggingPage
            // 
            this.enableUsbDebuggingPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.enableUsbDebuggingPage.AntiAlias = false;
            this.enableUsbDebuggingPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.enableUsbDebuggingPage.Controls.Add(this.labelX10);
            this.enableUsbDebuggingPage.Location = new System.Drawing.Point(7, 72);
            this.enableUsbDebuggingPage.Name = "enableUsbDebuggingPage";
            this.enableUsbDebuggingPage.PageDescription = "Enable USB debugging on your device";
            this.enableUsbDebuggingPage.PageTitle = "Enable USB debugging";
            this.enableUsbDebuggingPage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.enableUsbDebuggingPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.enableUsbDebuggingPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.enableUsbDebuggingPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.enableUsbDebuggingPage.TabIndex = 12;
            // 
            // labelX10
            // 
            // 
            // 
            // 
            this.labelX10.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX10.Location = new System.Drawing.Point(112, 32);
            this.labelX10.Name = "labelX10";
            this.labelX10.Size = new System.Drawing.Size(592, 208);
            this.labelX10.TabIndex = 0;
            this.labelX10.Text = resources.GetString("labelX10.Text");
            this.labelX10.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.labelX10.WordWrap = true;
            // 
            // manufacturerSelectionPage
            // 
            this.manufacturerSelectionPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manufacturerSelectionPage.AntiAlias = false;
            this.manufacturerSelectionPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.manufacturerSelectionPage.Controls.Add(this.cbManufacturer);
            this.manufacturerSelectionPage.Controls.Add(this.labelX4);
            this.manufacturerSelectionPage.Location = new System.Drawing.Point(7, 72);
            this.manufacturerSelectionPage.Name = "manufacturerSelectionPage";
            this.manufacturerSelectionPage.PageDescription = "Select the manufacturer of your device.";
            this.manufacturerSelectionPage.PageTitle = "Connect using USB cable";
            this.manufacturerSelectionPage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.manufacturerSelectionPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.manufacturerSelectionPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.manufacturerSelectionPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.manufacturerSelectionPage.TabIndex = 8;
            this.manufacturerSelectionPage.AfterPageDisplayed += new DevComponents.DotNetBar.WizardPageChangeEventHandler(this.OnManufacturerSelectionPageDisplayed);
            // 
            // cbManufacturer
            // 
            this.cbManufacturer.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.cbManufacturer.Border.Class = "ListViewBorder";
            this.cbManufacturer.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.cbManufacturer.ForeColor = System.Drawing.Color.Black;
            this.cbManufacturer.HideSelection = false;
            this.cbManufacturer.Location = new System.Drawing.Point(112, 48);
            this.cbManufacturer.MultiSelect = false;
            this.cbManufacturer.Name = "cbManufacturer";
            this.cbManufacturer.Size = new System.Drawing.Size(608, 208);
            this.cbManufacturer.TabIndex = 1;
            this.cbManufacturer.UseCompatibleStateImageBehavior = false;
            this.cbManufacturer.View = System.Windows.Forms.View.List;
            this.cbManufacturer.SelectedIndexChanged += new System.EventHandler(this.OnManufacturerSelectedIndexChanged);
            // 
            // labelX4
            // 
            this.labelX4.AutoSize = true;
            // 
            // 
            // 
            this.labelX4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX4.Location = new System.Drawing.Point(112, 32);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(193, 15);
            this.labelX4.TabIndex = 0;
            this.labelX4.Text = "Select the manufacturer of your device.";
            // 
            // manufacturerInfoPage
            // 
            this.manufacturerInfoPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manufacturerInfoPage.AntiAlias = false;
            this.manufacturerInfoPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.manufacturerInfoPage.Controls.Add(this.labelX9);
            this.manufacturerInfoPage.Controls.Add(this.lbManufacturerHelp);
            this.manufacturerInfoPage.FinishButtonEnabled = DevComponents.DotNetBar.eWizardButtonState.True;
            this.manufacturerInfoPage.Location = new System.Drawing.Point(7, 72);
            this.manufacturerInfoPage.Name = "manufacturerInfoPage";
            this.manufacturerInfoPage.NextButtonEnabled = DevComponents.DotNetBar.eWizardButtonState.True;
            this.manufacturerInfoPage.NextButtonVisible = DevComponents.DotNetBar.eWizardButtonState.True;
            this.manufacturerInfoPage.PageDescription = "Manufacturer specific connection assistance.";
            this.manufacturerInfoPage.PageTitle = "< Wizard step title >";
            this.manufacturerInfoPage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.manufacturerInfoPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.manufacturerInfoPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.manufacturerInfoPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.manufacturerInfoPage.TabIndex = 10;
            // 
            // labelX9
            // 
            this.labelX9.AutoSize = true;
            // 
            // 
            // 
            this.labelX9.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX9.Location = new System.Drawing.Point(112, 256);
            this.labelX9.Name = "labelX9";
            this.labelX9.Size = new System.Drawing.Size(211, 15);
            this.labelX9.TabIndex = 9;
            this.labelX9.Text = "Click Next if you\'re still not able to connect.";
            // 
            // lbManufacturerHelp
            // 
            // 
            // 
            // 
            this.lbManufacturerHelp.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbManufacturerHelp.Location = new System.Drawing.Point(112, 32);
            this.lbManufacturerHelp.Name = "lbManufacturerHelp";
            this.lbManufacturerHelp.Size = new System.Drawing.Size(600, 208);
            this.lbManufacturerHelp.TabIndex = 0;
            this.lbManufacturerHelp.Text = "-";
            this.lbManufacturerHelp.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.lbManufacturerHelp.WordWrap = true;
            this.lbManufacturerHelp.MarkupLinkClick += new DevComponents.DotNetBar.MarkupLinkClickEventHandler(this.OnMarkupLinkClick);
            // 
            // cannotConnectPage
            // 
            this.cannotConnectPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cannotConnectPage.AntiAlias = false;
            this.cannotConnectPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.cannotConnectPage.Controls.Add(this.sendToSupportProgress);
            this.cannotConnectPage.Controls.Add(this.cmdSendToSupport);
            this.cannotConnectPage.Controls.Add(this.tbDevice);
            this.cannotConnectPage.Controls.Add(this.labelX8);
            this.cannotConnectPage.Controls.Add(this.labelX7);
            this.cannotConnectPage.Controls.Add(this.tbEmail);
            this.cannotConnectPage.Controls.Add(this.tbName);
            this.cannotConnectPage.Controls.Add(this.labelX6);
            this.cannotConnectPage.Controls.Add(this.labelX5);
            this.cannotConnectPage.FinishButtonEnabled = DevComponents.DotNetBar.eWizardButtonState.False;
            this.cannotConnectPage.Location = new System.Drawing.Point(7, 72);
            this.cannotConnectPage.Name = "cannotConnectPage";
            this.cannotConnectPage.PageDescription = "If you still cannot connect your device, tell us about the device.";
            this.cannotConnectPage.PageTitle = "Connection still fails";
            this.cannotConnectPage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.cannotConnectPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.cannotConnectPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.cannotConnectPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.cannotConnectPage.TabIndex = 11;
            this.cannotConnectPage.AfterPageDisplayed += new DevComponents.DotNetBar.WizardPageChangeEventHandler(this.OnCannotConnectPageDisplayed);
            // 
            // sendToSupportProgress
            // 
            // 
            // 
            // 
            this.sendToSupportProgress.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.sendToSupportProgress.FocusCuesEnabled = false;
            this.sendToSupportProgress.Location = new System.Drawing.Point(688, 176);
            this.sendToSupportProgress.Name = "sendToSupportProgress";
            this.sendToSupportProgress.ProgressBarType = DevComponents.DotNetBar.eCircularProgressType.Dot;
            this.sendToSupportProgress.Size = new System.Drawing.Size(40, 40);
            this.sendToSupportProgress.Style = DevComponents.DotNetBar.eDotNetBarStyle.OfficeXP;
            this.sendToSupportProgress.TabIndex = 8;
            this.sendToSupportProgress.Visible = false;
            // 
            // cmdSendToSupport
            // 
            this.cmdSendToSupport.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdSendToSupport.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdSendToSupport.Location = new System.Drawing.Point(632, 232);
            this.cmdSendToSupport.Name = "cmdSendToSupport";
            this.cmdSendToSupport.Size = new System.Drawing.Size(104, 32);
            this.cmdSendToSupport.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdSendToSupport.TabIndex = 7;
            this.cmdSendToSupport.Text = "&Send";
            this.cmdSendToSupport.Click += new System.EventHandler(this.OnSendToSupportClick);
            // 
            // tbDevice
            // 
            this.tbDevice.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.tbDevice.Border.Class = "TextBoxBorder";
            this.tbDevice.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tbDevice.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbDevice.Location = new System.Drawing.Point(112, 168);
            this.tbDevice.Multiline = true;
            this.tbDevice.Name = "tbDevice";
            this.tbDevice.Size = new System.Drawing.Size(504, 96);
            this.tbDevice.TabIndex = 6;
            this.tbDevice.TextChanged += new System.EventHandler(this.OnCannotConnectionOptionChanged);
            // 
            // labelX8
            // 
            this.labelX8.AutoSize = true;
            // 
            // 
            // 
            this.labelX8.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX8.Location = new System.Drawing.Point(112, 152);
            this.labelX8.Name = "labelX8";
            this.labelX8.Size = new System.Drawing.Size(327, 15);
            this.labelX8.TabIndex = 5;
            this.labelX8.Text = "Exact description of your device and what you\'re tried to connect it:";
            // 
            // labelX7
            // 
            this.labelX7.AutoSize = true;
            // 
            // 
            // 
            this.labelX7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX7.Location = new System.Drawing.Point(112, 104);
            this.labelX7.Name = "labelX7";
            this.labelX7.Size = new System.Drawing.Size(100, 15);
            this.labelX7.TabIndex = 4;
            this.labelX7.Text = "Your email address:";
            // 
            // tbEmail
            // 
            this.tbEmail.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.tbEmail.Border.Class = "TextBoxBorder";
            this.tbEmail.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tbEmail.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbEmail.Location = new System.Drawing.Point(112, 120);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(504, 20);
            this.tbEmail.TabIndex = 3;
            this.tbEmail.TextChanged += new System.EventHandler(this.OnCannotConnectionOptionChanged);
            // 
            // tbName
            // 
            this.tbName.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.tbName.Border.Class = "TextBoxBorder";
            this.tbName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tbName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbName.Location = new System.Drawing.Point(112, 72);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(504, 20);
            this.tbName.TabIndex = 2;
            this.tbName.TextChanged += new System.EventHandler(this.OnCannotConnectionOptionChanged);
            // 
            // labelX6
            // 
            this.labelX6.AutoSize = true;
            // 
            // 
            // 
            this.labelX6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX6.Location = new System.Drawing.Point(112, 56);
            this.labelX6.Name = "labelX6";
            this.labelX6.Size = new System.Drawing.Size(59, 15);
            this.labelX6.TabIndex = 1;
            this.labelX6.Text = "Your name:";
            // 
            // labelX5
            // 
            this.labelX5.AutoSize = true;
            // 
            // 
            // 
            this.labelX5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX5.Location = new System.Drawing.Point(112, 32);
            this.labelX5.Name = "labelX5";
            this.labelX5.Size = new System.Drawing.Size(523, 15);
            this.labelX5.TabIndex = 0;
            this.labelX5.Text = "If you are not able to connect your device, please tell us about your device and " +
    "we\'ll be happy to assist you.";
            // 
            // connectionTypePage
            // 
            this.connectionTypePage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionTypePage.AntiAlias = false;
            this.connectionTypePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(221)))), ((int)(((byte)(238)))));
            this.connectionTypePage.Controls.Add(this.cbConnectViaWifi);
            this.connectionTypePage.Controls.Add(this.cbConnectViaUsb);
            this.connectionTypePage.Controls.Add(this.labelX11);
            this.connectionTypePage.Location = new System.Drawing.Point(7, 72);
            this.connectionTypePage.Name = "connectionTypePage";
            this.connectionTypePage.PageDescription = "Choose the type of connection you want to use to connect to your device.";
            this.connectionTypePage.PageTitle = "Choose connection type";
            this.connectionTypePage.Size = new System.Drawing.Size(795, 279);
            // 
            // 
            // 
            this.connectionTypePage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.connectionTypePage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.connectionTypePage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.connectionTypePage.TabIndex = 13;
            // 
            // labelX11
            // 
            // 
            // 
            // 
            this.labelX11.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX11.Location = new System.Drawing.Point(160, 56);
            this.labelX11.Name = "labelX11";
            this.labelX11.Size = new System.Drawing.Size(432, 23);
            this.labelX11.TabIndex = 0;
            this.labelX11.Text = "How do you want to connect to your device?";
            // 
            // cbConnectViaUsb
            // 
            // 
            // 
            // 
            this.cbConnectViaUsb.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.cbConnectViaUsb.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.cbConnectViaUsb.Checked = true;
            this.cbConnectViaUsb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConnectViaUsb.CheckValue = "Y";
            this.cbConnectViaUsb.Location = new System.Drawing.Point(184, 104);
            this.cbConnectViaUsb.Name = "cbConnectViaUsb";
            this.cbConnectViaUsb.Size = new System.Drawing.Size(144, 16);
            this.cbConnectViaUsb.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cbConnectViaUsb.TabIndex = 1;
            this.cbConnectViaUsb.Text = "Via &USB";
            // 
            // cbConnectViaWifi
            // 
            // 
            // 
            // 
            this.cbConnectViaWifi.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.cbConnectViaWifi.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.cbConnectViaWifi.Location = new System.Drawing.Point(184, 128);
            this.cbConnectViaWifi.Name = "cbConnectViaWifi";
            this.cbConnectViaWifi.Size = new System.Drawing.Size(144, 16);
            this.cbConnectViaWifi.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cbConnectViaWifi.TabIndex = 2;
            this.cbConnectViaWifi.Text = "Via &WIFI";
            // 
            // ConnectionWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 409);
            this.Controls.Add(this.wizard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionWizardForm";
            this.Text = "Device Connection Wizard";
            this.wizard.ResumeLayout(false);
            this.welcomePage.ResumeLayout(false);
            this.connectOverWifiPage.ResumeLayout(false);
            this.connectOverWifiPage.PerformLayout();
            this.enableUsbDebuggingPage.ResumeLayout(false);
            this.manufacturerSelectionPage.ResumeLayout(false);
            this.manufacturerSelectionPage.PerformLayout();
            this.manufacturerInfoPage.ResumeLayout(false);
            this.manufacturerInfoPage.PerformLayout();
            this.cannotConnectPage.ResumeLayout(false);
            this.cannotConnectPage.PerformLayout();
            this.connectionTypePage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Wizard wizard;
        private DevComponents.DotNetBar.WizardPage welcomePage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private DevComponents.DotNetBar.WizardPage manufacturerSelectionPage;
        private DevComponents.DotNetBar.WizardPage connectOverWifiPage;
        private DevComponents.DotNetBar.Controls.SwitchButton switchButtonIsRooted;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.Controls.SwitchButton switchButtonWifi;
        private DevComponents.DotNetBar.LabelX lbWifiResult;
        private DevComponents.DotNetBar.Controls.SwitchButton switchButtonAdbWifiInstalled;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.ButtonX cmdConnectOverWifi;
        private DevComponents.DotNetBar.LabelX lbWifiOtherOptions;
        private DevComponents.DotNetBar.Controls.ListViewEx cbManufacturer;
        private DevComponents.DotNetBar.LabelX labelX4;
        private DevComponents.DotNetBar.WizardPage manufacturerInfoPage;
        private DevComponents.DotNetBar.LabelX lbManufacturerHelp;
        private DevComponents.DotNetBar.WizardPage cannotConnectPage;
        private DevComponents.DotNetBar.ButtonX cmdSendToSupport;
        private DevComponents.DotNetBar.Controls.TextBoxX tbDevice;
        private DevComponents.DotNetBar.LabelX labelX8;
        private DevComponents.DotNetBar.LabelX labelX7;
        private DevComponents.DotNetBar.Controls.TextBoxX tbEmail;
        private DevComponents.DotNetBar.Controls.TextBoxX tbName;
        private DevComponents.DotNetBar.LabelX labelX6;
        private DevComponents.DotNetBar.LabelX labelX5;
        private DevComponents.DotNetBar.LabelX labelX9;
        private DevComponents.DotNetBar.WizardPage enableUsbDebuggingPage;
        private DevComponents.DotNetBar.LabelX labelX10;
        private CircularProgress sendToSupportProgress;
        private DevComponents.DotNetBar.WizardPage connectionTypePage;
        private CheckBoxX cbConnectViaWifi;
        private CheckBoxX cbConnectViaUsb;
        private DevComponents.DotNetBar.LabelX labelX11;
    }
}
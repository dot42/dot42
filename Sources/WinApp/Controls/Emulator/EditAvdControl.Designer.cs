namespace Dot42.Gui.Controls.Emulator
{
    partial class EditAvdControl
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
            this.tvSettings = new DevComponents.AdvTree.AdvTree();
            this.chKey = new DevComponents.AdvTree.ColumnHeader();
            this.chValue = new DevComponents.AdvTree.ColumnHeader();
            this.nodeName = new DevComponents.AdvTree.Node();
            this.cellNameValue = new DevComponents.AdvTree.Cell();
            this.nodeTarget = new DevComponents.AdvTree.Node();
            this.nodeSkin = new DevComponents.AdvTree.Node();
            this.nodeSkinBuiltin = new DevComponents.AdvTree.Node();
            this.nodeSkinCustom = new DevComponents.AdvTree.Node();
            this.nodeSdCard = new DevComponents.AdvTree.Node();
            this.nodeSdNone = new DevComponents.AdvTree.Node();
            this.nodeSdSize = new DevComponents.AdvTree.Node();
            this.nodeSdFile = new DevComponents.AdvTree.Node();
            this.nodeSnapshot = new DevComponents.AdvTree.Node();
            this.cell1 = new DevComponents.AdvTree.Cell();
            this.nodeHw = new DevComponents.AdvTree.Node();
            this.nodeConnector1 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();
            this.buttonPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cmdOk = new DevComponents.DotNetBar.ButtonX();
            this.cmdCancel = new DevComponents.DotNetBar.ButtonX();
            ((System.ComponentModel.ISupportInitialize)(this.tvSettings)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvSettings
            // 
            this.tvSettings.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvSettings.AllowDrop = true;
            this.tvSettings.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.tvSettings.BackgroundStyle.Class = "TreeBorderKey";
            this.tvSettings.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tvSettings.Columns.Add(this.chKey);
            this.tvSettings.Columns.Add(this.chValue);
            this.tvSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSettings.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.tvSettings.Location = new System.Drawing.Point(8, 8);
            this.tvSettings.Name = "tvSettings";
            this.tvSettings.Nodes.AddRange(new DevComponents.AdvTree.Node[] {
            this.nodeName,
            this.nodeTarget,
            this.nodeSkin,
            this.nodeSdCard,
            this.nodeSnapshot,
            this.nodeHw});
            this.tvSettings.NodesConnector = this.nodeConnector1;
            this.tvSettings.NodeStyle = this.elementStyle1;
            this.tvSettings.PathSeparator = ";";
            this.tvSettings.Size = new System.Drawing.Size(752, 278);
            this.tvSettings.Styles.Add(this.elementStyle1);
            this.tvSettings.TabIndex = 0;
            this.tvSettings.Text = "advTree1";
            // 
            // chKey
            // 
            this.chKey.Name = "chKey";
            this.chKey.Text = "Key";
            this.chKey.Width.Absolute = 150;
            // 
            // chValue
            // 
            this.chValue.ColumnName = "Value";
            this.chValue.Name = "chValue";
            this.chValue.StretchToFill = true;
            this.chValue.Text = "Value";
            this.chValue.Width.Absolute = 150;
            // 
            // nodeName
            // 
            this.nodeName.Cells.Add(this.cellNameValue);
            this.nodeName.Expanded = true;
            this.nodeName.Name = "nodeName";
            this.nodeName.Text = "Name";
            // 
            // cellNameValue
            // 
            this.cellNameValue.Name = "cellNameValue";
            this.cellNameValue.StyleMouseOver = null;
            // 
            // nodeTarget
            // 
            this.nodeTarget.Expanded = true;
            this.nodeTarget.Name = "nodeTarget";
            this.nodeTarget.Text = "Target";
            // 
            // nodeSkin
            // 
            this.nodeSkin.Expanded = true;
            this.nodeSkin.Name = "nodeSkin";
            this.nodeSkin.Nodes.AddRange(new DevComponents.AdvTree.Node[] {
            this.nodeSkinBuiltin,
            this.nodeSkinCustom});
            this.nodeSkin.Text = "Skin";
            // 
            // nodeSkinBuiltin
            // 
            this.nodeSkinBuiltin.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.nodeSkinBuiltin.CheckBoxVisible = true;
            this.nodeSkinBuiltin.Checked = true;
            this.nodeSkinBuiltin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.nodeSkinBuiltin.Expanded = true;
            this.nodeSkinBuiltin.Name = "nodeSkinBuiltin";
            this.nodeSkinBuiltin.Text = "Built-in";
            // 
            // nodeSkinCustom
            // 
            this.nodeSkinCustom.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.nodeSkinCustom.CheckBoxVisible = true;
            this.nodeSkinCustom.Expanded = true;
            this.nodeSkinCustom.Name = "nodeSkinCustom";
            this.nodeSkinCustom.Text = "Custom resolution";
            // 
            // nodeSdCard
            // 
            this.nodeSdCard.Expanded = true;
            this.nodeSdCard.Name = "nodeSdCard";
            this.nodeSdCard.Nodes.AddRange(new DevComponents.AdvTree.Node[] {
            this.nodeSdNone,
            this.nodeSdSize,
            this.nodeSdFile});
            this.nodeSdCard.Text = "SD Card";
            // 
            // nodeSdNone
            // 
            this.nodeSdNone.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.nodeSdNone.CheckBoxVisible = true;
            this.nodeSdNone.Checked = true;
            this.nodeSdNone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.nodeSdNone.Expanded = true;
            this.nodeSdNone.Name = "nodeSdNone";
            this.nodeSdNone.Text = "No SD card";
            // 
            // nodeSdSize
            // 
            this.nodeSdSize.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.nodeSdSize.CheckBoxVisible = true;
            this.nodeSdSize.Expanded = true;
            this.nodeSdSize.Name = "nodeSdSize";
            this.nodeSdSize.Text = "Size";
            // 
            // nodeSdFile
            // 
            this.nodeSdFile.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.nodeSdFile.CheckBoxVisible = true;
            this.nodeSdFile.Expanded = true;
            this.nodeSdFile.Name = "nodeSdFile";
            this.nodeSdFile.Text = "Use existing file";
            // 
            // nodeSnapshot
            // 
            this.nodeSnapshot.Cells.Add(this.cell1);
            this.nodeSnapshot.Expanded = true;
            this.nodeSnapshot.Name = "nodeSnapshot";
            this.nodeSnapshot.Text = "Snapshot support";
            // 
            // cell1
            // 
            this.cell1.CheckBoxVisible = true;
            this.cell1.Name = "cell1";
            this.cell1.StyleMouseOver = null;
            // 
            // nodeHw
            // 
            this.nodeHw.Expanded = true;
            this.nodeHw.Name = "nodeHw";
            this.nodeHw.Text = "Hardware options";
            // 
            // nodeConnector1
            // 
            this.nodeConnector1.LineColor = System.Drawing.SystemColors.ControlText;
            // 
            // elementStyle1
            // 
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // buttonPanel
            // 
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonPanel.ColumnCount = 3;
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.buttonPanel.Controls.Add(this.cmdOk, 1, 0);
            this.buttonPanel.Controls.Add(this.cmdCancel, 2, 0);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(8, 286);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(3);
            this.buttonPanel.RowCount = 1;
            this.buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.buttonPanel.Size = new System.Drawing.Size(752, 49);
            this.buttonPanel.TabIndex = 1;
            // 
            // cmdOk
            // 
            this.cmdOk.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdOk.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdOk.Location = new System.Drawing.Point(526, 6);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(107, 37);
            this.cmdOk.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdOk.TabIndex = 0;
            this.cmdOk.Text = "&OK";
            this.cmdOk.Click += new System.EventHandler(this.OnOkClick);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmdCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(639, 6);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(107, 37);
            this.cmdCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // EditAvdControl
            // 
            this.AcceptButton = this.cmdOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(768, 343);
            this.Controls.Add(this.tvSettings);
            this.Controls.Add(this.buttonPanel);
            this.DoubleBuffered = true;
            this.MinimizeBox = false;
            this.Name = "EditAvdControl";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Virtual Device";
            ((System.ComponentModel.ISupportInitialize)(this.tvSettings)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.AdvTree.AdvTree tvSettings;
        private DevComponents.AdvTree.NodeConnector nodeConnector1;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;
        private System.Windows.Forms.TableLayoutPanel buttonPanel;
        private DevComponents.DotNetBar.ButtonX cmdOk;
        private DevComponents.DotNetBar.ButtonX cmdCancel;
        private DevComponents.AdvTree.Node nodeName;
        private DevComponents.AdvTree.ColumnHeader chKey;
        private DevComponents.AdvTree.ColumnHeader chValue;
        private DevComponents.AdvTree.Node nodeTarget;
        private DevComponents.AdvTree.Node nodeSdCard;
        private DevComponents.AdvTree.Node nodeSdSize;
        private DevComponents.AdvTree.Node nodeSdFile;
        private DevComponents.AdvTree.Node nodeSdNone;
        private DevComponents.AdvTree.Node nodeSkin;
        private DevComponents.AdvTree.Node nodeSkinBuiltin;
        private DevComponents.AdvTree.Node nodeSkinCustom;
        private DevComponents.AdvTree.Node nodeHw;
        private DevComponents.AdvTree.Node nodeSnapshot;
        private DevComponents.AdvTree.Cell cell1;
        private DevComponents.AdvTree.Cell cellNameValue;
    }
}

namespace Dot42.Gui.Controls.Emulator
{
    partial class CreateAvdControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateAvdControl));
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lbName = new DevComponents.DotNetBar.LabelX();
            this.tbName = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.cmdCancel = new DevComponents.DotNetBar.ButtonX();
            this.cmdOK = new DevComponents.DotNetBar.ButtonX();
            this.lbTarget = new DevComponents.DotNetBar.LabelX();
            this.cbTarget = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.superValidator1 = new DevComponents.DotNetBar.Validator.SuperValidator();
            this.existsValidation = new DevComponents.DotNetBar.Validator.CustomValidator();
            this.nameValidator = new DevComponents.DotNetBar.Validator.CustomValidator();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.highlighter1 = new DevComponents.DotNetBar.Validator.Highlighter();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.BackColor = System.Drawing.Color.Transparent;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.89743F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lbName, 0, 1);
            this.tlpMain.Controls.Add(this.tbName, 1, 1);
            this.tlpMain.Controls.Add(this.cmdCancel, 3, 4);
            this.tlpMain.Controls.Add(this.cmdOK, 2, 4);
            this.tlpMain.Controls.Add(this.lbTarget, 0, 2);
            this.tlpMain.Controls.Add(this.cbTarget, 1, 2);
            this.tlpMain.Controls.Add(this.labelX1, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(494, 134);
            this.tlpMain.TabIndex = 0;
            // 
            // lbName
            // 
            this.lbName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbName.AutoSize = true;
            // 
            // 
            // 
            this.lbName.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbName.Location = new System.Drawing.Point(6, 29);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(35, 15);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // 
            // 
            this.tbName.Border.Class = "TextBoxBorder";
            this.tbName.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tlpMain.SetColumnSpan(this.tbName, 3);
            this.tbName.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbName.FocusHighlightEnabled = true;
            this.tbName.ForeColor = System.Drawing.Color.Black;
            this.tbName.Location = new System.Drawing.Point(127, 27);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(361, 20);
            this.tbName.TabIndex = 2;
            this.superValidator1.SetValidator1(this.tbName, this.existsValidation);
            this.superValidator1.SetValidator2(this.tbName, this.nameValidator);
            this.tbName.TextChanged += new System.EventHandler(this.OnNameTextChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(394, 98);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(93, 30);
            this.cmdCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdCancel.TabIndex = 6;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // cmdOK
            // 
            this.cmdOK.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdOK.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(295, 98);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(93, 30);
            this.cmdOK.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmdOK.TabIndex = 5;
            this.cmdOK.Text = "&OK";
            this.cmdOK.Click += new System.EventHandler(this.OnOkClick);
            // 
            // lbTarget
            // 
            this.lbTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbTarget.AutoSize = true;
            // 
            // 
            // 
            this.lbTarget.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbTarget.Location = new System.Drawing.Point(6, 55);
            this.lbTarget.Name = "lbTarget";
            this.lbTarget.Size = new System.Drawing.Size(92, 15);
            this.lbTarget.TabIndex = 3;
            this.lbTarget.Text = "Target framework:";
            // 
            // cbTarget
            // 
            this.tlpMain.SetColumnSpan(this.cbTarget, 3);
            this.cbTarget.DisplayMember = "Text";
            this.cbTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbTarget.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTarget.FormattingEnabled = true;
            this.cbTarget.ItemHeight = 14;
            this.cbTarget.Location = new System.Drawing.Point(127, 53);
            this.cbTarget.Name = "cbTarget";
            this.cbTarget.Size = new System.Drawing.Size(361, 20);
            this.cbTarget.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cbTarget.TabIndex = 4;
            // 
            // labelX1
            // 
            this.labelX1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelX1.AutoSize = true;
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(6, 6);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(334, 15);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "Enter the name of the new emulator and select it\'s target framework.";
            // 
            // superValidator1
            // 
            this.superValidator1.ContainerControl = this;
            this.superValidator1.ErrorProvider = this.errorProvider1;
            this.superValidator1.Highlighter = this.highlighter1;
            this.superValidator1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.superValidator1.SteppedValidation = true;
            // 
            // existsValidation
            // 
            this.existsValidation.ErrorMessage = "This name already exists";
            this.existsValidation.HighlightColor = DevComponents.DotNetBar.Validator.eHighlightColor.Red;
            this.existsValidation.ValuePropertyName = "Text";
            this.existsValidation.ValidateValue += new DevComponents.DotNetBar.Validator.ValidateValueEventHandler(this.OnValidateNameExists);
            // 
            // nameValidator
            // 
            this.nameValidator.ErrorMessage = "This name is not valid.";
            this.nameValidator.HighlightColor = DevComponents.DotNetBar.Validator.eHighlightColor.Red;
            this.nameValidator.ValuePropertyName = "Text";
            this.nameValidator.ValidateValue += new DevComponents.DotNetBar.Validator.ValidateValueEventHandler(this.OnValidateNameIsValid);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            this.errorProvider1.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider1.Icon")));
            // 
            // highlighter1
            // 
            this.highlighter1.ContainerControl = this;
            this.highlighter1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            // 
            // CreateAvdControl
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(494, 268);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 28);
            this.Name = "CreateAvdControl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Emulator";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private DevComponents.DotNetBar.LabelX lbName;
        private DevComponents.DotNetBar.Controls.TextBoxX tbName;
        private DevComponents.DotNetBar.ButtonX cmdCancel;
        private DevComponents.DotNetBar.ButtonX cmdOK;
        private DevComponents.DotNetBar.Validator.SuperValidator superValidator1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private DevComponents.DotNetBar.Validator.Highlighter highlighter1;
        private DevComponents.DotNetBar.Validator.CustomValidator existsValidation;
        private DevComponents.DotNetBar.Validator.CustomValidator nameValidator;
        private DevComponents.DotNetBar.LabelX lbTarget;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cbTarget;
        private DevComponents.DotNetBar.LabelX labelX1;
    }
}

namespace Dot42.Ide.WizardForms
{
    partial class AdditionalLibrariesControl
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
            this.tvLibs = new System.Windows.Forms.ListView();
            this.chLibName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLicense = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // tvLibs
            // 
            this.tvLibs.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvLibs.AllowDrop = true;
            this.tvLibs.BackColor = System.Drawing.SystemColors.Window;
            this.tvLibs.CheckBoxes = true;
            this.tvLibs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chLibName,
            this.chLicense});
            this.tvLibs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLibs.FullRowSelect = true;
            this.tvLibs.Location = new System.Drawing.Point(0, 0);
            this.tvLibs.Name = "tvLibs";
            this.tvLibs.Size = new System.Drawing.Size(559, 282);
            this.tvLibs.TabIndex = 0;
            this.tvLibs.UseCompatibleStateImageBehavior = false;
            this.tvLibs.View = System.Windows.Forms.View.Details;
            this.tvLibs.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.tvLibsItemChecked);
            // 
            // chLibName
            // 
            this.chLibName.Name = "chLibName";
            this.chLibName.Text = "Name";
            this.chLibName.Width = 150;
            // 
            // chLicense
            // 
            this.chLicense.Name = "chLicense";
            this.chLicense.Text = "License";
            this.chLicense.Width = 150;
            // 
            // AdditionalLibrariesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvLibs);
            this.Name = "AdditionalLibrariesControl";
            this.Size = new System.Drawing.Size(559, 282);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView tvLibs;
        private System.Windows.Forms.ColumnHeader chLibName;
        private System.Windows.Forms.ColumnHeader chLicense;

    }
}

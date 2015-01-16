
namespace Dot42.CryptoUI.Controls
{
	/// <summary>
    /// Summary description for SaveAsControl
	/// </summary>
    partial class SaveAsControl
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilename;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.tbFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbImport = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbFilename
            // 
            this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilename.Location = new System.Drawing.Point(112, 8);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.Size = new System.Drawing.Size(288, 20);
            this.tbFilename.TabIndex = 1;
            this.tbFilename.Click += new System.EventHandler(this.OnBrowseFileNameClick);
            this.tbFilename.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filename";
            // 
            // cbImport
            // 
            this.cbImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbImport.Checked = true;
            this.cbImport.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbImport.Location = new System.Drawing.Point(112, 40);
            this.cbImport.Name = "cbImport";
            this.cbImport.Size = new System.Drawing.Size(288, 24);
            this.cbImport.TabIndex = 2;
            this.cbImport.Text = "Import the certificate in my certificate store.";
            // 
            // SaveAsControl
            // 
            this.Controls.Add(this.cbImport);
            this.Controls.Add(this.tbFilename);
            this.Controls.Add(this.label1);
            this.Name = "SaveAsControl";
            this.Size = new System.Drawing.Size(408, 352);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private System.Windows.Forms.CheckBox cbImport;

	}
}


namespace Dot42.CryptoUI.Controls
{
	/// <summary>
	/// Summary description for SettingsControl.
	/// </summary>
    partial class KeyStoreSettingsControl 
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbPassword2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lbNotEqual;
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
            this.tbPassword = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
            this.tbPassword2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lbNotEqual = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// tbPassword
			// 
            this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tbPassword.Location = new System.Drawing.Point(112, 8);
			this.tbPassword.Name = "tbPassword";
			this.tbPassword.PasswordChar = '*';
			this.tbPassword.Size = new System.Drawing.Size(288, 20);
			this.tbPassword.TabIndex = 1;
			this.tbPassword.Text = "";
			this.tbPassword.TextChanged += new System.EventHandler(this.OnTextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Password";
			// 
			// tbPassword2
			// 
            this.tbPassword2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tbPassword2.Location = new System.Drawing.Point(112, 32);
			this.tbPassword2.Name = "tbPassword2";
			this.tbPassword2.PasswordChar = '*';
			this.tbPassword2.Size = new System.Drawing.Size(288, 20);
			this.tbPassword2.TabIndex = 2;
			this.tbPassword2.Text = "";
			this.tbPassword2.TextChanged += new System.EventHandler(this.OnTextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Re-type password";
			// 
			// lbNotEqual
			// 
			this.lbNotEqual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lbNotEqual.Location = new System.Drawing.Point(112, 56);
			this.lbNotEqual.Name = "lbNotEqual";
			this.lbNotEqual.Size = new System.Drawing.Size(288, 16);
			this.lbNotEqual.TabIndex = 4;
			this.lbNotEqual.Text = "Passwords are not equal!";
			this.lbNotEqual.Visible = false;
			// 
			// KeyStoreSettingsControl
			// 
			this.Controls.Add(this.lbNotEqual);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbPassword2);
			this.Controls.Add(this.tbPassword);
			this.Controls.Add(this.label1);
			//this.Header = "Security Settings";
			this.Name = "KeyStoreSettingsControl";
			this.Size = new System.Drawing.Size(408, 352);
			/*this.Summary = "Enter the password that gives access to the new certificate and click Next to cre" +
				"ate the certificate.";*/
			this.ResumeLayout(false);

		}
		#endregion

	}
}

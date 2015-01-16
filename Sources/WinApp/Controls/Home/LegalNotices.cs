using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Dot42.Shared.UI;

namespace Dot42.Gui.Controls.Home
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
    public class LegalNotices : AppForm
	{
		private Button cmdOk;
		private RichTextBox rbText;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LegalNotices()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Load the text
			LoadText();									  
		}

		/// <summary>
		/// Load the legal text into the textbox
		/// </summary>
		private void LoadText() 
		{
            try
            {
                byte[] data;
                var resName = typeof (Legal).FullName + ".rtf";
                using (var stream = GetType().Assembly.GetManifestResourceStream(resName))
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
                rbText.Text = Encoding.ASCII.GetString(data);
            }
            catch (Exception ex)
            {
                rbText.Text = string.Format("Failed to load legal information because {0}.", ex.Message);
            }
		}

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LegalNotices));
            this.cmdOk = new Button();
            this.rbText = new RichTextBox();
            this.SuspendLayout();
            // 
            // cmdOk
            // 
            this.cmdOk.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.cmdOk, "cmdOk");
            this.cmdOk.Name = "cmdOk";
            // 
            // rbText
            // 
            this.rbText.BackColor = System.Drawing.Color.White;
            this.rbText.DetectUrls = false;
            resources.ApplyResources(this.rbText, "rbText");
            this.rbText.Name = "rbText";
            this.rbText.ReadOnly = true;
            // 
            // LegalNotices
            // 
            this.AcceptButton = this.cmdOk;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cmdOk;
            this.Controls.Add(this.rbText);
            this.Controls.Add(this.cmdOk);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LegalNotices";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

		}
		#endregion

	}
}

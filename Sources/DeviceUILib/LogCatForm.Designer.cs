namespace Dot42.DeviceLib.UI
{
    partial class LogCatForm
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
            this.components = new System.ComponentModel.Container();
            this.logCatControl = new LogCatControl();
            this.SuspendLayout();
            // 
            // logCatControl
            // 
            this.logCatControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logCatControl.Location = new System.Drawing.Point(0, 0);
            this.logCatControl.Name = "logCatControl";
            this.logCatControl.Size = new System.Drawing.Size(886, 454);
            this.logCatControl.TabIndex = 0;
            // 
            // LogCatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 454);
            this.Controls.Add(this.logCatControl);
            this.MinimizeBox = false;
            this.Name = "LogCatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "View Device Log";
            this.ResumeLayout(false);

        }

        #endregion

        private LogCatControl logCatControl;
    }
}
namespace Dot42.Gui.Forms
{
    partial class ProgressForm<T>
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
            this.modalPanelContainer = new Dot42.Shared.UI.ModalPanelContainer();
            this.SuspendLayout();
            // 
            // modalPanelContainer
            // 
            this.modalPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modalPanelContainer.Location = new System.Drawing.Point(0, 0);
            this.modalPanelContainer.Name = "modalPanelContainer";
            this.modalPanelContainer.Size = new System.Drawing.Size(855, 433);
            this.modalPanelContainer.TabIndex = 0;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 433);
            this.ControlBox = false;
            this.Controls.Add(this.modalPanelContainer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ProgressForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Shared.UI.ModalPanelContainer modalPanelContainer;
    }
}
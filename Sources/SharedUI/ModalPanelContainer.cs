using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Dot42.Shared.UI
{
    public class ModalPanelContainer : ContainerControl
    {
        internal const int DefaultAnimationTime = 800;

        private readonly List<Control> modalPanels = new List<Control>();

        /// <summary>
        /// Shows the panel control in the center of the form and covers all non system controls making the panel effectively modal.
        /// </summary>
        /// <param name="panel">Control to show.</param>
        public void ShowModalPanel(Control panel)
        {
            Panel slidePanel = new Panel();
            slidePanel.Bounds = GetModalPanelBounds();

            slidePanel.Controls.Add(panel);
            this.Controls.Add(slidePanel);
            this.Controls.SetChildIndex(slidePanel, 0);
            modalPanels.Add(slidePanel);

            this.Update();
        }

        /// <summary>
        /// Hides the panel control that was previously shown using ShowModalPanel method.
        /// </summary>
        /// <param name="panel">Control to hide.</param>
        public void CloseModalPanel(Control panel)
        {
            Panel slidePanel = null;
            foreach (Control modalPanel in modalPanels)
            {
                if (modalPanel.Contains(panel))
                {
                    slidePanel = (Panel) modalPanel;
                    break;
                }
            }
            if (slidePanel == null)
                throw new ArgumentException("panel was not shown previously using ShowModalPanel method");

            slidePanel.Visible = false;
            slidePanel.Controls.Remove(panel);
            slidePanel.Parent.Controls.Remove(slidePanel);
            modalPanels.Remove(slidePanel);
            slidePanel.Dispose();
        }

        private Rectangle GetModalPanelBounds()
        {
            return new Rectangle(Point.Empty, Size);
        }
    }
}

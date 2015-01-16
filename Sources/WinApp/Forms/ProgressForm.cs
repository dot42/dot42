using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Dot42.Shared.UI;

namespace Dot42.Gui.Forms
{
    public partial class ProgressForm<T> : AppForm, IModalPanelForm
        where T : Control, IProgressControl
    {
        private readonly T control;
        private readonly Action<T> initialize;
        private readonly Stack<Control> modalControls = new Stack<Control>();
        private bool initialized;

        /// <summary>
        /// Designer ctor
        /// </summary>
        public ProgressForm()
            : this(null, null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProgressForm(T control, Action<T> initialize)
        {
            this.control = control;
            this.initialize = initialize;
            InitializeComponent();
        }

        /// <summary>
        /// Load control
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            UpdateControlsSizeAndLocation();
            base.OnLoad(e);
            initialized = true;
            if (control != null)
            {
                ShowModalPanel<T>(control, initialize);
            }
        }

        private Rectangle GetListControlBounds()
        {
            return new Rectangle(Point.Empty, modalPanelContainer.Size);
        }

        /// <summary>
        /// Set the bounds of the opened controls.
        /// </summary>
        private void UpdateControlsSizeAndLocation()
        {
            var bounds = GetListControlBounds();
            var modalControl = (modalControls.Count > 0) ? modalControls.Peek() : null;
            if (modalControl != null)
            {
                modalControl.Bounds = bounds;
            }
        }

        /// <summary>
        /// Update sizes
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            if (initialized)
                UpdateControlsSizeAndLocation();
            base.OnResize(e);
        }

        /// <summary>
        /// Show the given control as modal panel
        /// </summary>
        public void ShowModalPanel<TControl>(TControl x, Action<TControl> initialize)
            where TControl : Control, IProgressControl
        {
            Text = ((IProgressControl) x).Title;
            x.Bounds = GetListControlBounds();
            modalControls.Push(x);
            modalPanelContainer.ShowModalPanel(x);
            BeginInvoke(initialize, x);
        }

        /// <summary>
        /// Close the current modal panel
        /// </summary>
        void IModalPanelForm.CloseModalPanel()
        {
            var x = modalControls.Pop();
            modalPanelContainer.CloseModalPanel(x);
            if (modalControls.Count == 0)
            {
                Close();
            }
        }
    }
}

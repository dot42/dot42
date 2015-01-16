using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Dot42.Shared.UI
{
    public abstract class WindowPreferences
    {
        private readonly Form form;
        private readonly Size minSize;
        private bool boundsLoaded;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected WindowPreferences(Form form, Size minSize)
        {
            this.form = form;
            this.minSize = minSize;
        }

        /// <summary>
        /// Attach to the form's events.
        /// </summary>
        internal void Attach()
        {
            RestoreWindowBounds();
            form.LocationChanged += FormOnLocationChanged;
            form.SizeChanged += FormSizeChanged;
            form.Closed += FormOnClosed;            
        }

        /// <summary>
        /// Detach when form is closed.
        /// </summary>
        private void FormOnClosed(object sender, EventArgs eventArgs)
        {
            form.LocationChanged -= FormOnLocationChanged;
            form.SizeChanged -= FormSizeChanged;
            form.Closed -= FormOnClosed;
        }

        /// <summary>
        /// Form size has changed.
        /// </summary>
        private void FormSizeChanged(object sender, EventArgs e)
        {
            if (boundsLoaded)
            {
                // Save in prefs
                Maximized = (form.WindowState == FormWindowState.Maximized);
                Size = form.Size;
                UserPreferences.SaveNow();
            }
        }

        /// <summary>
        /// Form has changed it location.
        /// </summary>
        private void FormOnLocationChanged(object sender, EventArgs eventArgs)
        {
            if (boundsLoaded)
            {
                Location = form.Location;
                UserPreferences.SaveNow();
            }
        }

        /// <summary>
        /// Location of window
        /// </summary>
        public abstract Point Location { get; set; }

        /// <summary>
        /// Size of window
        /// </summary>
        public abstract Size Size { get; set; }

        /// <summary>
        /// Is window maximized
        /// </summary>
        public abstract bool Maximized { get; set; }

        /// <summary>
        /// Restore bounds from stored preferences.
        /// </summary>
        private void RestoreWindowBounds()
        {
            // Restore preferences
            try
            {
                var location = Location;
                if (Maximized)
                {
                    form.Location = location;
                    form.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    var sz = Size;
                    sz.Width = Math.Max(minSize.Width, sz.Width);
                    sz.Height = Math.Max(minSize.Height, sz.Height);

                    var bounds = new Rectangle(location, sz);
                    var visible = Screen.AllScreens.Aggregate(false, (current, screen) => current | screen.WorkingArea.IntersectsWith(bounds));
                    if (visible)
                    {
                        form.Bounds = bounds;
                    }
                }
            }
            finally
            {
                this.boundsLoaded = true;

                // Set minimum size latest to avoid redraw problems.
                form.MinimumSize = minSize;
            }
        }


    }
}

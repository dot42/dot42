using System;
using Dot42.Ide;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Services
{
    internal sealed class IdeSelectionContainer : IIdeSelectionContainer, ISelectionContainer
    {
        private readonly IServiceProvider serviceProvider;
        private ITrackSelection trackSelection;
        private object selectedObject;

        /// <summary>
        /// Default ctor
        /// </summary>
        public IdeSelectionContainer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the selection tracker.
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get { return trackSelection ?? (trackSelection = serviceProvider.GetService(typeof(STrackSelection)) as ITrackSelection); }
        }

        /// <summary>
        /// Update the selected object
        /// </summary>
        public object SelectedObject
        {
            private get { return selectedObject; }
            set
            {
                selectedObject = value;
                var trackSel = TrackSelection;
                if (trackSel != null)
                    trackSel.OnSelectChange(this);
            }
        }

        int ISelectionContainer.CountObjects(uint dwFlags, out uint pc)
        {
            pc = (uint) ((SelectedObject != null) ? 1 : 0);
            return VSConstants.S_OK;
        }

        int ISelectionContainer.GetObjects(uint dwFlags, uint cObjects, object[] apUnkObjects)
        {
            if (cObjects >= 1)
                apUnkObjects[0] = selectedObject;
            return VSConstants.S_OK;
        }

        int ISelectionContainer.SelectObjects(uint cSelect, object[] apUnkSelect, uint dwFlags)
        {
            return VSConstants.S_OK;
        }
    }
}

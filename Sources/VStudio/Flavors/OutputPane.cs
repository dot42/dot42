using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Provide access to Dot42 output pane.
    /// </summary>
    internal class OutputPane
    {
        private IVsOutputWindowPane outputPane;

        /// <summary>
        /// Write a message to the output pane.
        /// </summary>
        public void LogLine(string message)
        {
            Load();
            outputPane.OutputStringThreadSafe(message + Environment.NewLine);
        }

        /// <summary>
        /// Make sure the output pane is loaded.
        /// </summary>
        public void Load()
        {
            if (outputPane == null)
            {
                outputPane = VSUtilities.ServiceProvider().GetOutputPane(new Guid(GuidList.Strings.guidDot42OutputPane), "dot42", true, false);
                outputPane.Activate();
            }
        }
    }
}

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Provide access to Dot42 output pane.
    /// </summary>
    internal class OutputPane
    {
        private readonly Guid _guid;
        private readonly string _title;
        private IVsOutputWindowPane outputPane;

        public OutputPane(Guid guid = default(Guid), string title="dot42")
        {
            _guid = guid;
            _title = title;
        }

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
                Guid guid = _guid;
                if (_guid == default(Guid))
                {
                    outputPane = VSUtilities.ServiceProvider().GetOutputPane(new Guid(GuidList.Strings.guidDot42OutputPane), _title, true, false);
                    outputPane.Activate();
                }
                else
                {
                    outputPane = VSUtilities.ServiceProvider().GetOutputPane(_guid, _title, true, false);
                }
            }
        }
    }
}

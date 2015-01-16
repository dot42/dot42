using Dot42.Utility;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Base class for all ADB tool based tasks.
    /// </summary>
    public abstract class AdbToolTask : ToolTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected AdbToolTask(bool showOutput)
        {
            Utils.InitializeLocations();           
            if (showOutput)
            {
                StandardOutputImportance = "High";
            }
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            return Locations.Adb;
        }

        protected override string ToolName
        {
            get { return "adb.exe"; }
        }

        protected override void LogToolCommand(string message)
        {
            base.LogToolCommand(message);
        }

        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param><param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (!string.IsNullOrEmpty(singleLine))
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }
    }
}

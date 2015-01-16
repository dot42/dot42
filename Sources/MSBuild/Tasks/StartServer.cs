using System.Diagnostics;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Start an ADB server.
    /// </summary>
    public class StartServer: AdbToolTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StartServer()
            : base(true/*false*/)
        {
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendSwitch("start-server");

            return builder.ToString();
        }

        /// <summary>
        /// Start server needs a custom process execution.
        /// </summary>
        public override bool Execute()
        {
            var startInfo = new ProcessStartInfo(GenerateFullPathToTool(), GenerateCommandLineCommands());
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            var p = Process.Start(startInfo);
            p.WaitForExit();
            return true;
        }
    }
}

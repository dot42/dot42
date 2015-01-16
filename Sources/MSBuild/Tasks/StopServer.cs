using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Stop an ADB server.
    /// </summary>
    public class StopServer: AdbToolTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StopServer()
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

            builder.AppendSwitch("kill-server");

            return builder.ToString();
        }
    }
}

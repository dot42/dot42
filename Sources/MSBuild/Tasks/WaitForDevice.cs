using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Wait until a device is online.
    /// </summary>
    public class WaitForDevice : AdbToolTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public WaitForDevice()
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

            builder.AppendSwitch("wait-for-device");

            return builder.ToString();
        }
    }
}

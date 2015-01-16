using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to uninstall a package.
    /// </summary>
    public class UninstallApk : AdbToolTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public UninstallApk()
            : base(true)
        {
        }

        /// <summary>
        /// Name of the package to uninstall
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendSwitch("uninstall");
            builder.AppendSwitch(PackageName);

            return builder.ToString();
        }
    }
}

using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to run the instrumentation.
    /// </summary>
    public class InstrumentApk : AdbToolTask
    {
        private const string DefaultTestRunner = "android.test.InstrumentationTestRunner";

        private int errorCount;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InstrumentApk()
            : base(true)
        {
            Wait = true;
            TestRunner = DefaultTestRunner;
        }

        /// <summary>
        /// Name of the package to run
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Name of the test runner
        /// </summary>
        [DefaultValue(DefaultTestRunner)]
        public string TestRunner { get; set; }

        /// <summary>
        /// Wait until finished?
        /// </summary>
        [DefaultValue(true)]
        public bool Wait { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendSwitch("shell");
            builder.AppendSwitch("am");
            builder.AppendSwitch("instrument");
            if (Wait)
            {
                builder.AppendSwitch("-w");
            }
            builder.AppendSwitch(PackageName + "/" + TestRunner);

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (singleLine.Contains("FAILURE") || singleLine.Contains("VerifyError") || (singleLine.Contains("Process crashed")) || singleLine.Contains("AndroidException"))
            {
                base.LogEventsFromTextOutput(singleLine, MessageImportance.High);
                errorCount++;
            }
            else
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }

        public override bool Execute()
        {
            var result = base.Execute();
            if ((!result) || (errorCount != 0))
            {
                Log.LogError("Instrumentation failed");
                return false;
            }
            return true;
        }
    }
}

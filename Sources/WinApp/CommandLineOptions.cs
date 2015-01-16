using System;
using Dot42.Utility;
using Mono.Options;

namespace Dot42.Gui
{
    internal class CommandLineOptions
    {
        private readonly OptionSet options;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CommandLineOptions(string[] args)
        {
            options = new OptionSet {
                { ToolOptions.Help, "Show usage", v => ShowHelp = true },
                { ToolOptions.SamplesFolder, "Specify folder containing sample projects", v => SamplesFolder = v },
            };
            options.Parse(args);
        }

        public bool ShowHelp { get; private set; }
        public string SamplesFolder { get; private set; }

        /// <summary>
        /// Show usage
        /// </summary>
        public void Usage()
        {
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

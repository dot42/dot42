using System;
using Dot42.Utility;
using Mono.Options;

namespace Dot42.TemplateBuilder
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
                { ToolOptions.OutputFolder, "Output folder", v => OutputFolder = v },
            };
            var list = options.Parse(args);
            TemplateFolder = (list.Count == 1) ? list[0] : null;

            ShowHelp |= (string.IsNullOrEmpty(TemplateFolder));
            ShowHelp |= (string.IsNullOrEmpty(OutputFolder));
        }

        public bool ShowHelp { get; private set; }
        public string TemplateFolder { get; private set; }
        public string OutputFolder { get; private set; }

        /// <summary>
        /// Show usage
        /// </summary>
        public void Usage()
        {
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

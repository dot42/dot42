using System;
using Dot42.Utility;
using Mono.Options;

namespace Dot42.FrameworkBuilder
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
                {ToolOptions.Help, "Show usage", v => ShowHelp = true},
                {ToolOptions.InputJar, "Set input android.jar", v => FrameworkJar = v},
                {ToolOptions.InputAttrsXml, "Set input attrs.xml", v => AttrsXml = v},
                {ToolOptions.InputSourceProperties, "Set input source.properties", v => SourceProperties = v},
                {ToolOptions.OutputFolder, "Set output folder", v => OutputFolder = v},
                {ToolOptions.OutputFolderFile, "Set output folder file", v => OutputFolderFile = v},
                {ToolOptions.DoxygenXmlFolder, "Set doxygen XML folder", v => DoxygenXmlFolder = v}, 
                {ToolOptions.ForwardAssembliesFolder, "Set folder with forward assembly sources",v => ForwardAssembliesFolder = v},
                {ToolOptions.PublicKeyToken, "Set PublicKeyToken for FrameworkList.xml", v => PublicKeyToken = v},
                {ToolOptions.VersionOnly, "Generate version only", v => VersionOnly = true},
            };
            options.Parse(args);

            ShowHelp |= (string.IsNullOrEmpty(FrameworkJar));
            ShowHelp |= (string.IsNullOrEmpty(AttrsXml));
            ShowHelp |= (string.IsNullOrEmpty(SourceProperties));
            ShowHelp |= (string.IsNullOrEmpty(OutputFolder));
            ShowHelp |= (string.IsNullOrEmpty(ForwardAssembliesFolder));
            //ShowHelp |= (string.IsNullOrEmpty(PublicKeyToken));
        }

        public bool ShowHelp { get; private set; }
        public string SourceProperties { get; private set; }
        public string FrameworkJar { get; private set; }
        public string AttrsXml { get; private set; }
        public string OutputFolder { get; private set; }
        public string OutputFolderFile { get; private set; }
        public string DoxygenXmlFolder { get; private set; }
        public string FrameworkListFolder { get; private set; }
        public string ForwardAssembliesFolder { get; private set; }
        public string PublicKeyToken { get; private set; }
        public bool VersionOnly { get; private set; }

        /// <summary>
        /// Show usage
        /// </summary>
        public void Usage()
        {
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;
using Mono.Options;

namespace Dot42.BuildTools
{
    internal class CommandLineOptions
    {
        private readonly OptionSet options;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CommandLineOptions(string[] args)
        {
            EnumTypeNames = new List<string>();
            options = new OptionSet {
                { ToolOptions.Help, "Show usage", v => ShowHelp = true },
                //  Template update
                { ToolOptions.VsTemplatePath, "Set template path to update", v => TemplatePath = v },
                { ToolOptions.Target, "Set platform target", v => Target = v },
                // Documentation builder
                { ToolOptions.InputFrameworksFolder, "Set folder of frameworks to document", v => FrameworksFolder = v },
                { ToolOptions.OutputFolder, "Set output path of documentation", v => OutputFolder = v },
                // Sample Uninstall-Delete script
                { ToolOptions.SamplesFolder, "Set samples folder", v => SamplesFolder = v },
                { ToolOptions.Script, "Set script path", v => ScriptPath = v },
                // System ID builder
                //{ ToolOptions.InputFrameworksFolder, "Set folder of frameworks to document", v => FrameworksFolder = v },
                { ToolOptions.SystemIdSourceFile, "Set system ID's source file", v => SystemIdSourcePath = v },
                // Debugger Exceptions Snippet Builder
                { ToolOptions.DebuggerExceptionsSourceFile, "Set Debugger Exceptions source file", v => DebuggerExceptionsSourceFile = v },
                // Enum value names builder
                { ToolOptions.InputAssembly, "Set assembly input path", v => InputAssembly = v },
                { ToolOptions.EnumTypeName, "Set enum type name", v => EnumTypeNames.Add(v) },
                // Fix corlib
                { ToolOptions.FixAssembly, "Fix mscorlib.dll", v => FixAssemblyPath = v },
                // Find listener interface
                { ToolOptions.FindApiEnhancementsAssembly, "Find API enhancements", v => FindApiEnhancements = v },
                // Find methods with Runnable arguments
                { ToolOptions.UninstallAPK, "Uninstall APK", v => UninstallAPK = v },
                // Find missing type forwarders
                { ToolOptions.CheckForwarders, "Check forwarders", v => CheckForwardersAssembly = v },
            };
            options.Parse(args);

            var hasTemplatePath = !string.IsNullOrEmpty(TemplatePath);
            var hasTarget = !string.IsNullOrEmpty(Target);
            var hasFrameworksFolder = !string.IsNullOrEmpty(FrameworksFolder);
            var hasSamplesFolder = !string.IsNullOrEmpty(SamplesFolder);
            var hasSystemIdSource = !string.IsNullOrEmpty(SystemIdSourcePath);
            var hasDebuggerExceptionsSourceFile = !string.IsNullOrEmpty(DebuggerExceptionsSourceFile);
            var hasInputAssembly = !string.IsNullOrEmpty(InputAssembly);
            var hasEnumTypeNames = EnumTypeNames.Any();
            var hasFixAssembly = !string.IsNullOrEmpty(FixAssemblyPath);
            var hasFindApiEnhancements = !string.IsNullOrEmpty(FindApiEnhancements);
            var hasUninstallAPK = !string.IsNullOrEmpty(UninstallAPK);
            var hasCheckForwardersAssembly = !string.IsNullOrEmpty(CheckForwardersAssembly);

            ShowHelp |= !(hasTemplatePath || hasFrameworksFolder || hasSamplesFolder || hasSystemIdSource || hasDebuggerExceptionsSourceFile || hasInputAssembly || hasFixAssembly || hasFindApiEnhancements || hasUninstallAPK || hasCheckForwardersAssembly);
            ShowHelp |= hasTemplatePath && string.IsNullOrEmpty(Target);
            ShowHelp |= hasFrameworksFolder && (!(hasSystemIdSource || hasDebuggerExceptionsSourceFile)) && string.IsNullOrEmpty(OutputFolder);
            ShowHelp |= hasSamplesFolder && string.IsNullOrEmpty(ScriptPath);
            ShowHelp |= hasSystemIdSource && string.IsNullOrEmpty(FrameworksFolder);
            ShowHelp |= hasDebuggerExceptionsSourceFile && string.IsNullOrEmpty(FrameworksFolder);
            ShowHelp |= hasInputAssembly && (string.IsNullOrEmpty(OutputFolder) || !hasEnumTypeNames);
        }

        public bool ShowHelp { get; private set; }
        public string TemplatePath { get; private set; }
        public string FrameworksFolder { get; private set; }
        public string OutputFolder { get; private set; }
        public string SamplesFolder { get; private set; }
        public string ScriptPath { get; private set; }
        public string SystemIdSourcePath { get; private set; }
        public string DebuggerExceptionsSourceFile { get; private set; }
        public string InputAssembly { get; private set; }
        public List<string> EnumTypeNames { get; private set; }
        public string FixAssemblyPath { get; private set; }
        public string FindApiEnhancements { get; private set; }
        public string UninstallAPK { get; private set; }
        public string Target { get; private set; }
        public string CheckForwardersAssembly { get; private set; }

        /// <summary>
        /// Show usage
        /// </summary>
        public void Usage()
        {
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

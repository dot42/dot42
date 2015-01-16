using System;
using System.Collections.Generic;
using Dot42.Compiler;
using Dot42.Utility;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TallComponents.Common.Util;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Base class for all Dot42 compiler based tasks.
    /// </summary>
    public abstract class Dot42CompilerTask : Task
    {
        /// <summary>
        /// Generate the 'command line' arguments.
        /// </summary>
        protected abstract string[] GenerateArguments();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected Dot42CompilerTask()
        {
            Utils.InitializeLocations();
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                // Get arguments
                var args = GenerateArguments();

                if (null == args)
                {
                    // If we get no result, the derived class should have set the log messages.
                    return false;
                }
                else
                {
                    if (0 == args.Length)
                    {
                        // If we get no parameters, we don't need to call the compiler.
                        return true;
                    }
                    else
                    {
                        // Show arguments (low level)
                        var builder = new CommandLineBuilder();
                        foreach (var arg in args)
                        {
                            if (arg.StartsWith("--"))
                                builder.AppendSwitch(arg);
                            else
                                builder.AppendFileNameIfNotNull(arg);
                        }

                        Log.LogCommandLine(MessageImportance.Normal, builder.ToString());

                        // Invoke
                        return CompilerService.Execute(args, Log);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        /// <summary>
        /// Add all items to the command line
        /// </summary>
        protected static void AddResources(IEnumerable<ITaskItem> items, ToolOption argument, List<string> builder)
        {
            if (items == null) return;
            foreach (var x in items)
            {
                builder.Add(argument.AsArg());
                builder.Add(x.ItemSpec);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to create WCF proxy source code for a given assembly.
    /// </summary>
    public class GenerateWcfProxy : Dot42CompilerTask
    {
        /// <summary>
        /// Input assembly
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Folders containing reference assemblies
        /// </summary>
        public ITaskItem[] ReferenceFolders { get; set; }

        /// <summary>
        /// File path of the generated source code (.cs)
        /// </summary>
        [Required]
        public ITaskItem GeneratedSourcePath { get; set; }

        protected override string[] GenerateArguments()
        {
            var builder = new List<string>();

            if ((Assemblies != null) && (Assemblies.Length > 0))
            {
                foreach (var asm in Assemblies)
                {
                    builder.Add(ToolOptions.WcfProxyInputAssembly.AsArg());
                    builder.Add(asm.ItemSpec);
                }
            }

            if (GeneratedSourcePath != null)
            {
                builder.Add(ToolOptions.WcfProxyGeneratedSourcePath.AsArg());
                builder.Add(GeneratedSourcePath.ItemSpec);
            }

            if (ReferenceFolders != null)
            {
                foreach (var x in ReferenceFolders.Where(x => x != null))
                {
                    builder.Add(ToolOptions.ReferenceFolder.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            builder.AddTarget();
            return builder.ToArray();
        }
    }
}

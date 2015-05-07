using System;
using System.IO;
using System.Linq;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.BuildTools.Tasks
{
    [TaskName("assembly-api-compare")]
    public class AssemblyApiCompareTask : Task
    {
        [TaskAttribute("assembly", Required = true)]
        public string Assembly { get; set; }

        [TaskAttribute("reference", Required = true)]
        public string Reference { get; set; }

        [TaskAttribute("output", Required = true)]
        public string Output { get; set; }

        protected override void ExecuteTask()
        {
            // Maybe this is supposed to compare the 
            // API of Dot42 to the original one by Microsoft.
            // Reference points to the "Sources\ApiReferences"
            // folder.

            // Then again it seems to update the very same files in
            // ApiReferences, so I'm not really sure what this is 
            // supposed to do. The "Sources\ApiReferences" does
            // not contains the documentaion.

            Log(Level.Warning, "NOT IMPLEMENTED. Assembly={0} Reference={1} Output={2}", Assembly, Reference, Output==Reference?"(same as reference)" : Output);
        }
    }
}

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.BuildTools.Tasks
{
    [TaskName("assembly-reference-check")]
    public class AssemblyReferenceCheckTask : Task
    {
        [BuildElement("assemblies", Required = true)]
        public FileSet Assemblies { get; set; }

        public AssemblyReferenceCheckTask()
        {
            Assemblies= new FileSet();
        }
        protected override void ExecuteTask()
        {
            Log(Level.Warning, "not implemented");
        }
    }
}

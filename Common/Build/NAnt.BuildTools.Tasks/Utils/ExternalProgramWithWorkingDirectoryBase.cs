using System.Diagnostics;
using System.IO;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

namespace NAnt.BuildTools.Tasks.Utils
{
    public abstract class ExternalProgramWithWorkingDirectoryBase : ExternalProgramBase
    {
        [TaskAttribute("workingdir", Required = false)]
        public DirectoryInfo WorkingDirectory { get; set; }
        
        public override string ProgramArguments { get { return ""; } }

        protected override void PrepareProcess(Process process)
        {
            base.PrepareProcess(process);
            if (WorkingDirectory != null)
                process.StartInfo.WorkingDirectory = WorkingDirectory.FullName;
        }
    }
}

using System.IO;
using NAnt.BuildTools.Tasks.Utils;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.BuildTools.Tasks
{
    [TaskName("peverify")]
    public class PeVerifyTask : Task
    {

        [BuildElement("assemblies", Required = true)]
        public FileSet Assemblies { get; set; }

        public PeVerifyTask()
        {
            Assemblies= new FileSet();
        }
        protected override void ExecuteTask()
        {
            string peverify_exe = "peverify";

            try
            {
                peverify_exe = Properties.ExpandProperties("${peverify}", Location.UnknownLocation);
            }
            catch { }
            

            foreach (var assembly in Assemblies.Includes)
            {
                string path = Path.Combine(Assemblies.BaseDirectory.FullName, assembly);
                string args = string.Format(" {0} /unique /nologo ", path);

                if (!Verbose) args += " /quiet ";

                int resultCode;
                if((resultCode = Run.RunAndLog("peverify", args, Log)) != 0)
                    throw new BuildException("pe verify error code " + resultCode);
            }
        }
    }
}

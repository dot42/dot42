using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAnt.BuildTools.Tasks.IPC;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.BuildTools.Tasks
{
    /// <summary>
    /// this lets you build with devenv.com
    /// </summary>
    [TaskName("devenv")]
    public class DevEnvTask : Task
    {
        [ElementName("property")]
        public class Property : Element
        {
            [TaskAttribute("name")]
            public string PropertyName { get; set; }

            [TaskAttribute("value")]
            public string PropertyValue { get; set; }
        }

        [TaskAttribute("project", Required = true)]
        public string ProjectPath { get; set; }

        [TaskAttribute("target", Required = true)]
        public string Target { get; set; }

        [TaskAttribute("verbosity", Required = false)]
        public string Verbosity { get; set; }

        [BuildElementArray("property", Required = false)]
        public List<Property> BuildProperties { get; set; }

        public DevEnvTask()
        {
            BuildProperties = new List<Property>();
        }
        protected override void ExecuteTask()
        {
            var devenv = Properties.ExpandProperties("${devenv}", Location.UnknownLocation);

            var configuration = BuildProperties.Single(b => b.PropertyName == "Configuration").PropertyValue;
            var args = string.Format("{0} /{1} {2} ", ProjectPath, Target, configuration);

            var exitCode = Run.RunAndLog(devenv, args, Log);

            if (exitCode != 0)
                throw new BuildException("devenv failed with error code " + exitCode);
        }
    }
}

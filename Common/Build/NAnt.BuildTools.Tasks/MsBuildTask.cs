using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAnt.BuildTools.Tasks.IPC;
using NAnt.BuildTools.Tasks.Utils;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.BuildTools.Tasks
{
    /// <summary>
    ///  this calls msbuild
    /// </summary>
    [TaskName("msbuild")]
    public class MsBuildTask : Task
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

        public MsBuildTask()
        {
            BuildProperties = new List<Property>();
        }
        protected override void ExecuteTask()
        {
            var msbuild = "msbuild.exe";

            string props = "";

            if (BuildProperties.Count > 0)
            {
                props = "/p:\"" + string.Join("\";\"", BuildProperties.Select(p => p.PropertyName + "=" + p.PropertyValue)) + "\"";
            }
            
            var args = string.Format("/nologo /target:{0} {1} ", Target, props);

            if (!string.IsNullOrEmpty(Verbosity))
                args += " /verbosity:" + Verbosity;

            args += " " + ProjectPath;

            var exitCode = Run.RunAndLog(msbuild, args, Log);

            if (exitCode != 0)
                throw new BuildException("msbuild failed with error code " + exitCode);
        }
    }
}

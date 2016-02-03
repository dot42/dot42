using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAnt.BuildTools.Tasks.Utils;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.BuildTools.Tasks
{
    /// <summary>
    ///  this calls msbuild
    /// </summary>
    [TaskName("msbuild")]
    public class MsBuildTask : ExternalProgramWithWorkingDirectoryBase
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
        public FileInfo ProjectPath { get; set; }

        [TaskAttribute("target", Required = false)]
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
            ExeName = "msbuild.exe";

            Arguments.Add(new Argument("/nologo"));
            
            if(Target != null)
                Arguments.Add(new Argument("/target:" + Target));

            if (BuildProperties.Count > 0)
            {
                string props = "";
                props = "/p:\"" + string.Join("\";\"", BuildProperties.Select(p => p.PropertyName + "=" + p.PropertyValue)) + "\"";
                Arguments.Add(new Argument(props));
            }

            if (!string.IsNullOrEmpty(Verbosity))
                Arguments.Add(new Argument("/verbosity:" + Verbosity));

            Arguments.Add(new Argument(ProjectPath));

            base.ExecuteTask();
        }
    }
}

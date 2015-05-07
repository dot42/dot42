using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NAnt.BuildTools.Tasks.Utils;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.BuildTools.Tasks
{
    [TaskName("innosetup")]
    public class InnoSetupTask : ExternalProgramWithWorkingDirectoryBase
    {
        [TaskAttribute("script", Required = true)]
        public FileInfo Script { get; set; }

        [TaskAttribute("defines", Required = false)]
        public string Defines { get; set; }

        protected override void ExecuteTask()
        {
            ExeName = FindInnoSetupCompiler("iscc.exe");

            Arguments.Add(new Argument(Script));
            if (!Verbose) Arguments.Add(new Argument("/Q"));

            foreach(var def in Defines.Split(';')
                                      .Select(d => d.Trim())
                                      .Where(d => !string.IsNullOrEmpty(d)))
            {
                Arguments.Add(new Argument("/d" + def));
            }

            base.ExecuteTask();
        }

        private string FindInnoSetupCompiler(string innosetup)
        {
            int step = 0;
            try
            {
                // Try to get the path from the registry.
                //[HKEY_CLASSES_ROOT\InnoSetupScriptFile\shell\open\command]
                // @="\"C:\\Program Files (x86)\\Inno Setup 5\\Compil32.exe\" \"%1\""
                string opencmd = RegHelper.GetRegistryValue(
                    @"InnoSetupScriptFile\shell\open\command",
                    null, RegistryHive.ClassesRoot);
                
                ++step;
                string compiler = opencmd.Split(new[] {'"'}, StringSplitOptions.RemoveEmptyEntries)[0];

                ++step;
                string path = Path.GetDirectoryName(compiler);
                
                ++step;
                var isccpath = Path.Combine(path, innosetup);
                
                ++step;
                if (!File.Exists(isccpath))
                    throw new FileNotFoundException();

                innosetup = isccpath;
            }
            catch (Exception ex)
            {
                Log(Level.Warning, "Unable to find path to iscc.exe through registry ({0}). Trying %PATH%. Error was: {1} ",
                    step, ex.Message);
            }
            return innosetup;
        }
    }
}

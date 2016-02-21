using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.ApkSpy.IPC;
using Dot42.DexLib;

namespace Dot42.ApkSpy.Disassembly
{
    internal class BaksmaliDisassembler
    {
        private readonly ISpySettings _settings;

        public BaksmaliDisassembler(ISpySettings settings)
        {
            _settings = settings;
        }

        public string Disassemble(ClassDefinition def)
        {
            if (string.IsNullOrEmpty(_settings.BaksmaliCommand))
                return "#ERROR: command to run not set.";

            StringBuilder ret = new StringBuilder();

            try
            {
                // get a temporary directoy
                string tempPath = Path.GetTempFileName();
                File.Delete(tempPath);
                Directory.CreateDirectory(tempPath);

                string dexFileName = Path.Combine(tempPath, "classes.dex");

                // create dex with one class definition
                // NOTE: it would be better if we could get hold of
                //       the actual binary representation.
                //       or use the original apk/dex and limit what baksmali 
                //       processes.
                var dex = new Dex();
                dex.AddClass(def);
                dex.Write(dexFileName);

                // run baksmali
                var cmd = GetBacksmaliCommand(_settings, dexFileName, tempPath);

                ret.AppendLine("# processed with: " + cmd);
                ret.AppendLine("#");

                int retCode;
                List<string> output = new List<string>();
                if ((retCode = Run.System(output, "{0}", cmd)) != 0)
                    ret.AppendLine("# return code: " + retCode);

                // process eventual errors
                output.ForEach(p=>ret.AppendFormat("# output: {0}\n", p));
                    
                // find .smali file
                var filenames = Directory.EnumerateFiles(tempPath, "*.smali", SearchOption.AllDirectories).ToList();


                if (!filenames.Any())
                {
                    ret.AppendLine("# ERROR: baksmali did not produce any output files.");
                }

                if (filenames.Count > 1)
                    ret.AppendLine("# NOTE: baksmali produced multiple files");


                IEnumerable<string> contents = filenames.Select(n => File.ReadAllText(Path.Combine(tempPath, n)));
                ret.Append(string.Join("\n\n\n\n-----------\n\n\n\n", contents));

                // clean up. (don't clean up on exception, to let the user inspect what happened)
                Directory.Delete(tempPath, true);

                return ret.ToString();
            }
            catch (Exception ex)
            {
                return "# ERROR: " + ex;
            }

        }

        public static string GetBacksmaliCommand(ISpySettings settings, string dexFileName, string outputPath)
        {
            return string.Format("{0} {1} \"{2}\" -o \"{3}\"", settings.BaksmaliCommand, settings.BaksmaliParameters, dexFileName, outputPath);
        }
    }
}

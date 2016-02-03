using System;
using System.IO;
using System.Text;
using NAnt.BuildTools.Tasks.IPC;
using NAnt.Core;

namespace NAnt.BuildTools.Tasks.Utils
{
    public static class Run
    {
        public static int RunAndLog(string appFileName, string arguments, Action<Level, string> log)
        {
            var proc = new ConsoleProcess
            {
                Arguments = arguments,
                AppFileName = appFileName,
                Encoding = Encoding.UTF8
            };
            proc.StderrLineRead += line => log(Level.Error, line);
            proc.StdoutLineRead += line => log(Level.Info, line);

            log(Level.Verbose, string.Format("cmdline: {0} {1}", appFileName, arguments));

            proc.Start();
            proc.Wait();

            return proc.ExitCode;
        }

        public static int RunAndThrowOnError(string appFileName, string arguments, Action<Level, string> log)
        {
            int ret = RunAndLog(appFileName, arguments, log);
            if(ret != 0)
                throw  new BuildException(string.Format("{0} exited with error {1} (0x{2:X})", Path.GetFileName(appFileName), ret, ret));
            return ret;
        }
    }
}

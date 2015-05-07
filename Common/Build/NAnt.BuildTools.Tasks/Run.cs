using System;
using System.Text;
using NAnt.BuildTools.Tasks.IPC;
using NAnt.Core;

namespace NAnt.BuildTools.Tasks
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
    }
}

using System.Collections.Generic;

namespace Dot42.ApkSpy.IPC
{
    public static class Run
    {
        public static int System(string cmdlineFormat, params object[] args)
        {
            string cmdline = string.Format(cmdlineFormat, args);
            return Command("cmd.exe", "/c " + cmdline);
        }

        public static int System(IList<string> stdout, IList<string> stderr, string cmdlineFormat, params object[] args)
        {
            string cmdline = string.Format(cmdlineFormat, args);
            return Command("cmd.exe", "/c " + cmdline, stdout, stderr);
        }

        public static int System(IList<string> stdoutAndErr, string cmdlineFormat, params object[] args)
        {
            string cmdline = string.Format(cmdlineFormat, args);
            return Command("cmd.exe", "/c " + cmdline, stdoutAndErr);
        }


        public static int Command(string command, string arguments)
        {
            ConsoleProcess proc = new ConsoleProcess();
            proc.Arguments = arguments;
            proc.AppFileName = command;
            proc.Start();
            proc.Wait();
            return proc.ExitCode;
        }
        
        public static int Command(string command, string arguments, IList<string> stdout, IList<string> stderr)
        {
            ConsoleProcess proc = new ConsoleProcess();
            proc.Arguments = arguments;
            proc.AppFileName = command;
            proc.StderrLineRead += l => { if (stderr == null) return; lock (stderr) stderr.Add(l); };
            proc.StdoutLineRead += l => { if (stdout == null) return; lock (stdout) stdout.Add(l); };
            proc.Start();
            proc.Wait();
            return proc.ExitCode;
        }

        public static int Command(string command, string arguments, IList<string> stdoutAndErr)
        {
            ConsoleProcess proc = new ConsoleProcess();
            proc.Arguments = arguments;
            proc.AppFileName = command;
            proc.StderrLineRead += l => { lock (stdoutAndErr) stdoutAndErr.Add(l); };
            proc.StdoutLineRead += l => { lock (stdoutAndErr) stdoutAndErr.Add(l); };
            proc.Start();
            proc.Wait();
            return proc.ExitCode;
        }

    }
}

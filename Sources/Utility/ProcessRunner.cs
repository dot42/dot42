using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dot42.Utility
{
    /// <summary>
    /// Helper used to run command line apps.
    /// </summary>
    public class ProcessRunner
    {
        private static readonly char[] escapeChars = new[] { ' ', '\"' };
        private readonly string command;
        private readonly List<string> arguments;        

        /// <summary>
        /// Build a runner to run the given command with the given arguments.
        /// </summary>
        public ProcessRunner(string command, params string[] arguments)
            : this(command, (IEnumerable<string>)arguments)
        {
        }

        /// <summary>
        /// Build a runner to run the given command with the given arguments.
        /// </summary>
        public ProcessRunner(string command, IEnumerable<string> arguments)
        {
            this.command = command;
            this.arguments = arguments.ToList();
        }

        /// <summary>
        /// Action to call with log data
        /// </summary>
        public Action<string> Logger { get; set; }

        /// <summary>
        /// Should the command be shown?
        /// </summary>
        public bool LogCommand { get; set; }

        /// <summary>
        /// Run and return the exit code.
        /// </summary>
        public int Run()
        {
            return Run(-1);
        }

        /// <summary>
        /// Run and return the exit code.
        /// </summary>
        public int Run(int timeout)
        {
            var stdoutLock = new object();
            var stdout = new StringBuilder();
            var p = new Process();
            var logger = Logger;
            p.OutputDataReceived += (s, x) => { lock (stdoutLock) { stdout.AppendLine(x.Data); OnOutput(x.Data); if (logger != null) logger(x.Data); } };
            p.ErrorDataReceived += (s, x) => { lock (stdoutLock) { stdout.AppendLine(x.Data); OnOutput(x.Data); if (logger != null) logger(x.Data); } };
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = GetArguments(arguments);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            if (LogCommand && (logger != null))
            {
                logger(string.Format("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments));
            }

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit(timeout);
            lock (stdoutLock)
            {
                Output = stdout.ToString();
            }
            return p.HasExited ? p.ExitCode : -1;
        }

        /// <summary>
        /// Called when new output is available.
        /// </summary>
        protected virtual void OnOutput(string line)
        {
            // Override me
        }

        /// <summary>
        /// Output of the command.
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Is it needed to escape the given argument?
        /// </summary>
        public static bool ContainEscapeCharacter(string argument)
        {
            return (argument.IndexOfAny(escapeChars) > 0);
        }

        /// <summary>
        /// Escape arguments (as needed)
        /// </summary>
        public static string QuoteArgument(string argument)
        {
            return ContainEscapeCharacter(argument) ? string.Join(string.Empty, "\"", argument, "\"") : argument;
        }

        /// <summary>
        /// Escape arguments (as needed)
        /// </summary>
        private static string GetArguments(IEnumerable<string> arguments)
        {
            var sb = new StringBuilder();
            foreach (var x in arguments)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(QuoteArgument(x));
            }
            return sb.ToString();
        }
    }
}

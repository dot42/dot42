using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.BuildTools.Tasks
{
    public enum ActionType
    {
        Start,
        Close,
    }

    /// <summary>
    /// This is probably rather meant to log ADB instead of build output.
    /// </summary>
    [TaskName("record")]
    public class RecordTask : Task
    {
        private static readonly ConcurrentDictionary<string, TextWriter> Loggers = new ConcurrentDictionary<string, TextWriter>();

        [TaskAttribute("name", Required = true)]
        public FileInfo LogFile { get; set; }

        [TaskAttribute("action", Required = true)]
        public ActionType ActionType { get; set; }

        protected override void ExecuteTask()
        {
            if (ActionType == ActionType.Start)
            {
                Loggers.AddOrUpdate(LogFile.FullName, OpenLog, (key, val) => val);

                Project.MessageLogged += OnMessageLogged;    
            }
            else
            {
                TextWriter logFile;
                Loggers.TryRemove(LogFile.FullName, out logFile);

                if (logFile != null)
                {
                    logFile.WriteLine(FormatMessage("I", "closing logfile"));
                    logFile.Dispose();
                }
            }
        }


        private static void OnMessageLogged(object sender, BuildEventArgs e)
        {
            if (Loggers.IsEmpty)
            {
                // tiny race.
                ((Project)sender).MessageLogged -= OnMessageLogged;
                return;
            }

            var msg = FormatMessage(e);
            
            if (msg == null)
                return;
            
            foreach (var log in Loggers)
            {
                try
                {
                    log.Value.WriteLine(msg);
                }
                catch (Exception)
                {
                }
                
            }
        }

        private static string FormatMessage(BuildEventArgs e)
        {
            var level = e.MessageLevel.ToString().Substring(0, 1);
            return FormatMessage(level, e.Message);
        }

        private static string FormatMessage(string level, string msg)
        {
            return string.Format("{0:HH:mm:ss.ff}|{1}|{2}", DateTime.Now, level, msg);
        }

        private static TextWriter OpenLog(string filename)
        {
            var file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);
            var writer = new StreamWriter(file, Encoding.UTF8);
            writer.WriteLine(FormatMessage("I", "opened logfile"));
            return writer;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace TallComponents.Common.Util
{
    public static class ErrorLog
    {
        /// <summary>
        /// Dump all errors in an exception log
        /// </summary>
        /// <param name="errors"></param>
        public static void DumpErrors(IEnumerable<Exception> errors)
        {
            lock (typeof(ErrorLog))
            {
                var assembly = typeof(ErrorLog).Assembly;
                var folder = GetLogFolder();
                var path = Path.Combine(folder, Path.GetFileNameWithoutExtension(assembly.Location) + ".log");
                using (var writer = new StreamWriter(path, true))
                {
                    foreach (var ex in errors)
                    {
                        writer.WriteLine("[{0}] {1} ({2})", DateTime.Now, assembly.GetName().Name, assembly.GetName().Version);
                        writer.WriteLine("Commandline: {0}", Environment.CommandLine);
                        writer.WriteLine("OS: {0}", Environment.OSVersion.VersionString);
                        writer.WriteLine("CLR: {0}", Environment.Version);
                        writer.WriteLine("Exception: {0}", ex.Message);
                        writer.WriteLine(ex.StackTrace);
                        var inner = ex.InnerException;
                        while (inner != null)
                        {
                            writer.WriteLine("Inner exception: {0}", inner.Message);
                            writer.WriteLine(inner.StackTrace);
                            inner = inner.InnerException;
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Dump the given error in an exception log
        /// </summary>
        public static void DumpError(Exception error)
        {
            if (error != null)
            {
                DumpErrors(new[] { error });
            }
        }

        /// <summary>
        /// Gets the folder in which error reports are to be written.
        /// </summary>
        private static string GetLogFolder()
        {
            var folder = Environment.GetEnvironmentVariable("DOT42REPORTS");
            if (folder == null)
            {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(folder))
                    folder = null;
            }

            if (folder == null)
            {
                folder = Environment.CurrentDirectory;
            }

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return folder;
        }
    }
}

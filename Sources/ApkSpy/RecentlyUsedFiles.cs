using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Dot42.ApkSpy
{
    internal static class RecentlyUsedFiles
    {
        private const string FILE_X = "file";
        private const string TREEPATH = "treepath";
        private const string WINDOWSIZE = "size";
        private const string WINDOWLOCATION = "location";
        private const int maxNumberOfFiles = 10;
        private const string registryPath = @"Software\TallComponents\Dot42\Mru";
        private static readonly Size DefaultWindowSize = new Size(972, 567);
        private static readonly Point DefaultWindowLocation = new Point(100, 100);
        private static readonly TypeConverter sizeConverter = TypeDescriptor.GetConverter(typeof(Size));
        private static readonly TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));

        /// <summary>
        /// Add the given path to the MRU list
        /// </summary>
        internal static void Add(string path)
        {
            var list = Load();
            list.Remove(path);
            list.Insert(0, path);
            Save(list);
        }

        /// <summary>
        /// Gets all recently used files.
        /// </summary>
        internal static IEnumerable<string> Files
        {
            get { return Load().Where(File.Exists); }
        }

        /// <summary>
        /// Gets/sets the last known path in the tree of the last opened file.
        /// </summary>
        internal static string LastTreePath
        {
            get { return GetValue(TREEPATH) ?? string.Empty; }
            set { SetValue(TREEPATH, value); }
        }

        /// <summary>
        /// Size of the main window
        /// </summary>
        internal static Size WindowSize
        {
            get
            {
                var sz = GetValue(WINDOWSIZE);
                if (string.IsNullOrEmpty(sz))
                    return DefaultWindowSize;
                try
                {
                    return (Size) sizeConverter.ConvertFromString(sz);
                }
                catch
                {
                    return DefaultWindowSize;
                }
            }
            set { SetValue(WINDOWSIZE, sizeConverter.ConvertToString(value)); }
        }

        /// <summary>
        /// Location of the main window
        /// </summary>
        internal static Point WindowLocation
        {
            get
            {
                var loc = GetValue(WINDOWLOCATION);
                if (string.IsNullOrEmpty(loc))
                    return DefaultWindowLocation;
                try
                {
                    return (Point)pointConverter.ConvertFromString(loc);
                }
                catch
                {
                    return DefaultWindowLocation;
                }
            }
            set { SetValue(WINDOWLOCATION, pointConverter.ConvertToString(value)); }
        }

        /// <summary>
        /// Load the MRU list.
        /// </summary>
        private static List<string> Load()
        {
            var list = new List<string>();
            try
            {
                for (int i = 0; i < maxNumberOfFiles; i++)
                {
                    var path = GetValue(FILE_X + i);
                    if (!string.IsNullOrEmpty(path))
                    {
                        list.Add(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Loading MRU from Registry failed: " + ex.Message);
            }
            return list;
        }


        /// <summary>
        /// Save the changes to the registry
        /// </summary>
        private static void Save(List<string> files)
        {
            try
            {
                // Clear & write items
                for (int i = 0; i < maxNumberOfFiles; i++)
                {
                    // Remove first
                    SetValue(FILE_X + i, (i < files.Count) ? files[i] : null);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Saving MRU to Registry failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets/sets the last known path in the tree of the last opened file.
        /// </summary>
        private static string GetValue(string valueName)
        {
            string result = null;
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(registryPath);
                if (key != null)
                {
                    using (key)
                    {
                        result = key.GetValue(valueName) as string;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Loading MRU from Registry failed: " + ex.Message);
            }
            return result ?? string.Empty;
        }

        private static void SetValue(string valueName, string value)
        {
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(registryPath);
                if (key != null)
                {
                    using (key)
                    {
                        if (string.IsNullOrEmpty(value))
                            key.DeleteValue(valueName, false); 
                        else
                            key.SetValue(valueName, value);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Save MRU from Registry failed: " + ex.Message);
            }
        }
    }
}

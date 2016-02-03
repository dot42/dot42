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
    internal static class SettingsPersitor
    {
        private const string FILE_X = "file";
        private const string TREEPATH = "treepath";
        private const string WINDOWSIZE = "size";
        private const string WINDOWLOCATION = "location";
        private const string BAKSMALI_ENABLE = "baksmali_enable";
        private const string BAKSMALI_COMMAND = "baksmali_command";
        private const string BAKSMALI_PARAMETERS = "baksmali_parameters";
        private const string EMBED_SOURCE_CODE_POSITIONS = "embed_source_code_positions";
        private const string EMBED_SOURCE_CODE = "embed_source_code";
        private const string SHOW_CONTROL_FLOW = "show_control_flow";
        private const string FULL_TYPE_NAMES = "full_type_names";
        
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

        internal static bool EnableBaksmali
        {
            get
            {
                var loc = GetValue(BAKSMALI_ENABLE);
                if (string.IsNullOrEmpty(loc))
                    return false;
                try
                {
                    return int.Parse(loc) != 0;
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                SetValue(BAKSMALI_ENABLE, value?"1" : "0");
            }
        }

        internal static string BaksmaliCommand
        {
            get
            {
                return GetValue(BAKSMALI_COMMAND);
            }
            set
            {
                SetValue(BAKSMALI_COMMAND, value);
            }
        }

        internal static string BaksmaliParameters
        {
            get
            {
                return GetValue(BAKSMALI_PARAMETERS);
            }
            set
            {
                SetValue(BAKSMALI_PARAMETERS, value);
            }
        }

        public static bool EmbedSourceCodePositions
        {
            get { return GetValue(EMBED_SOURCE_CODE_POSITIONS, "0") != "0"; }
            set { SetValue(EMBED_SOURCE_CODE_POSITIONS, value? "1" : "0");}
        }

        public static bool EmbedSourceCode
        {
            get { return GetValue(EMBED_SOURCE_CODE, "0") != "0"; }
            set { SetValue(EMBED_SOURCE_CODE, value ? "1" : "0"); }
        }

        public static bool ShowControlFlow
        {
            get { return GetValue(SHOW_CONTROL_FLOW, "0") != "0"; }
            set { SetValue(SHOW_CONTROL_FLOW, value ? "1" : "0"); }
        }

        public static bool FullTypeNames
        {
            get { return GetValue(FULL_TYPE_NAMES, "0") != "0"; }
            set { SetValue(FULL_TYPE_NAMES, value ? "1" : "0"); }
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
        private static string GetValue(string valueName, string @default="")
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
            return result ?? @default;
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

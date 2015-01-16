using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dot42.Utility
{
    /// <summary>
    /// Wrapper for .ini properties files.
    /// </summary>
    public class IniFile
    {
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public IniFile()
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public IniFile(string path)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Select(StripComment).Where(x => !string.IsNullOrEmpty(x)))
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    properties[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        /// <summary>
        /// Gets/sets a property.
        /// Null is returned if not found.
        /// </summary>
        public string this[string key]
        {
            get 
            {
                string value;
                return properties.TryGetValue(key, out value) ? value : null;
            }
            set
            {
                if (value == null)
                {
                    properties.Remove(key);
                }
                else
                {
                    properties[key] = value;
                }
            }
        }

        /// <summary>
        /// Does a property for the given key exist?
        /// </summary>
        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        /// <summary>
        /// Get a value. 
        /// If not found, an argument exception is thrown.
        /// </summary>
        public string GetValueThrowOnError(string key)
        {
            var result = this[key];
            if (string.IsNullOrEmpty(result))
                throw new ArgumentException(string.Format("{0} not found", key));
            return result;
        }

        /// <summary>
        /// Get an integer value. 
        /// If not found, false is returned, true otherwise.
        /// </summary>
        public bool TryGetIntValue(string key, out int value)
        {
            value = 0;
            var result = this[key];
            if (string.IsNullOrEmpty(result))
                return false;
            return int.TryParse(result, out value);
        }

        /// <summary>
        /// Gets all available keys.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get { return properties.Keys; }
        }

        /// <summary>
        /// Save to the given file.
        /// The containing folder must exist.
        /// </summary>
        public void Save(string path)
        {
            var lines = properties.OrderBy(x => x.Key).Select(x => string.Format("{0}={1}", x.Key, x.Value)).ToArray();
            File.WriteAllText(path, string.Join("\n", lines), Encoding.ASCII);
        }

        /// <summary>
        /// Return the given line without comments
        /// </summary>
        private static string StripComment(string line)
        {
            var index = line.IndexOf('#');
            if (index < 0)
                return line.Trim();
            return line.Substring(0, index).Trim();
        }
    }
}

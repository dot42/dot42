using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;

namespace Dot42.AvdLib
{
    /// <summary>
    /// Manage all AVD's on the system
    /// </summary>
    public class AvdManager : IEnumerable<Avd>
    {
        private readonly List<Avd> avds = new List<Avd>();
        private readonly string rootFolder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AvdManager()
        {
            rootFolder = Locations.Avds;
            if (Directory.Exists(rootFolder))
            {
                var infoFiles = Directory.GetFiles(rootFolder, "*" + AvdConstants.AvdInfoExtension);
                avds.AddRange(infoFiles.Select(x => new Avd(rootFolder, Path.GetFileNameWithoutExtension(x))));
            }
        }

        /// <summary>
        /// Does an AVD with the given name exist?.
        /// </summary>
        public bool Contains(string name)
        {
            return avds.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Create a new AVD using the given name.
        /// </summary>
        public Avd Create(string name)
        {
            if (Contains(name))
                throw new ArgumentException("Name already exists");
            var avd = new Avd(rootFolder, name);
            avd.Save();
            avds.Add(avd);
            return avd;
        }

        /// <summary>
        /// Remove the given AVD and all its data.
        /// </summary>
        public void Remove(Avd avd)
        {
            if (!avds.Contains(avd))
                throw new ArgumentException("Unknown avd");
            avd.Remove(rootFolder);
            avds.Remove(avd);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Avd> GetEnumerator()
        {
            return avds.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

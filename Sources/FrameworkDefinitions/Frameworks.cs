using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;

namespace Dot42.FrameworkDefinitions
{
    public sealed class Frameworks : IEnumerable<FrameworkInfo>
    {
        private readonly List<FrameworkInfo> frameworks = new List<FrameworkInfo>();

        /// <summary>
        /// Single instance
        /// </summary>
        public static readonly Frameworks Instance = new Frameworks();

        /// <summary>
        /// Default ctor
        /// </summary>
        private Frameworks()
        {
            var root = Locations.Frameworks;
#if DEBUG
            //root = @"C:\Program Files\Dot42\Frameworks";
#endif
            if (Directory.Exists(root))
            {
                frameworks.AddRange(Directory.GetDirectories(root).Select(x => new FrameworkInfo(x)).OrderByDescending(x => x.Descriptor.ApiLevel));
            }
        }

        /// <summary>
        /// Gets the frameworks root folder.
        /// </summary>
        public static string Root
        {
            get
            {
                var root = Locations.Frameworks;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
                    return Environment.CurrentDirectory;
                return root;
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public Frameworks(string root)
        {
            if (Directory.Exists(root))
            {
                frameworks.AddRange(Directory.GetDirectories(root).Select(x => new FrameworkInfo(x)).OrderByDescending(x => x.Descriptor.ApiLevel));
            }
        }

        /// <summary>
        /// Gets the framework that describes the given minimum API level best.
        /// </summary>
        /// <returns>Null if not found</returns>
        public FrameworkInfo GetBySdkVersion(int minSdkVersion)
        {
            if (minSdkVersion < 0)
                return null;
            var result = frameworks.LastOrDefault(x => x.Descriptor.ApiLevel >= minSdkVersion);
            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<FrameworkInfo> GetEnumerator()
        {
            return frameworks.GetEnumerator();
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

        /// <summary>
        /// Retreive information about the newest framework in this collection
        /// </summary>
        /// <remarks>
        /// The API level in the Framework.ini file is checked.
        /// </remarks>
        /// <returns>The <see cref="FrameworkInfo"/> describing the newest framework in this collection.</returns>
        public FrameworkInfo GetNewestVersion()
        {
            FrameworkInfo result = null;
            int maxLevel = 0;
            foreach (FrameworkInfo info in this)
            {
                if (maxLevel < info.Descriptor.ApiLevel)
                {
                    maxLevel = info.Descriptor.ApiLevel;
                    result = info;
                }
            }

            return result;
        }
    }
}

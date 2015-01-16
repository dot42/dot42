using System.Collections.Generic;
using System.IO;
using Dot42.Utility;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    internal static class Utils
    {
#if ANDROID
        internal const Targets Target = Targets.Android;
#elif BB
        internal const Targets Target = Targets.BlackBerry;
#endif

        /// <summary>
        /// Setup the Target of the <see cref="Locations"/> class.
        /// </summary>
        internal static void InitializeLocations()
        {
            Locations.Target = Target;
        }

        /// <summary>
        /// Create a list of unique folders from the given items.
        /// </summary>
        internal static IEnumerable<string> GetReferenceFolders(this ITaskItem[] items)
        {
            var result = new HashSet<string>();
            foreach (var item in items)
            {
                if (item == null)
                    continue;
                var path = Path.GetFullPath(item.ItemSpec);
                if (File.Exists(path))
                {
                    result.Add(Path.GetDirectoryName(path));
                }
                else if (Directory.Exists(path))
                {
                    result.Add(path);
                }
            }
            return result;
        }
    }
}

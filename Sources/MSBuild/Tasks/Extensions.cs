using System.Collections.Generic;
using Dot42.Utility;

namespace Dot42.MSBuild.Tasks
{
    internal static class Extensions
    {
        /// <summary>
        /// Add the -target option to the given tool arguments builder.
        /// </summary>
        internal static void AddTarget(this List<string> builder)
        {
#if BB
            builder.Add(ToolOptions.Target.AsArg());
            builder.Add("BlackBerry");
#endif
        }
    }
}

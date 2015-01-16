using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// JvmClasslib related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Mark the given reference reachable
        /// </summary>
        internal static void MarkReachable(this AbstractReference memberRef, IReachableContext context)
        {
            if ((memberRef != null) && (!memberRef.IsReachable))
            {
                memberRef.SetReachable(context);
            }
        }
    }
}

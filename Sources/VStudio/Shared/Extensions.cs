using System;
using EnvDTE;
using Microsoft.VisualStudio.Text;

namespace Dot42.VStudio.Shared
{
    /// <summary>
    /// Editor extensions methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the project item from the given buffer.
        /// </summary>
        internal static ProjectItem GetProjectItem(this ITextBuffer buffer, IServiceProvider serviceProvider)
        {
            var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
            if (document == null)
                return null;
            var dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            if (dte == null)
                throw new InvalidOperationException("Cannot get DTE");
            return dte.Solution.FindProjectItem(document.FilePath);
        }

        /// <summary>
        /// Gets the MSBuild item type of a given project item.
        /// Returns null if not found.
        /// </summary>
        internal static string GetProjectItemType(this ProjectItem item, IServiceProvider serviceProvider)
        {
            if ((item == null) || (item.Properties == null))
                return null;
            var itemTypeProp = item.Properties.Item("ItemType");
            return (itemTypeProp != null) ? itemTypeProp.Value as string : null;
        }

        /// <summary>
        /// Gets the MSBuild item type of a given text buffer.
        /// Returns null if not found.
        /// </summary>
        internal static string GetProjectItemType(this ITextBuffer buffer, IServiceProvider serviceProvider)
        {
            return GetProjectItemType(buffer.GetProjectItem(serviceProvider), serviceProvider);
        }
    }
}

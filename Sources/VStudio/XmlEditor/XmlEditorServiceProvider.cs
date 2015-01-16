using System;
using System.IO;
using System.Reflection;

namespace Dot42.VStudio.XmlEditor
{
    /// <summary>
    /// Provider of the version independent XmlEditorService.
    /// </summary>
    public static class XmlEditorServiceProvider
    {
        private static IXmlEditorService editorService;

        /// <summary>
        /// Get the editor service.
        /// </summary>
        public static IXmlEditorService GetEditorService(IServiceProvider serviceProvider, string dteVersion)
        {
            if (editorService != null)
                return editorService;

            var myPath = typeof (XmlEditorServiceProvider).Assembly.Location;
            string extension = null;

            if (dteVersion.StartsWith("10."))
                extension = ".vs10";
            else if (dteVersion.StartsWith("11."))
                extension = ".vs11";
            else if (dteVersion.StartsWith("12."))
                extension = ".vs12";
            else
                throw new InvalidOperationException("Unknown Visual Studio version: " + dteVersion);

            var folder = Path.GetDirectoryName(myPath);
            var name = Path.GetFileNameWithoutExtension(myPath) + extension + ".dll";
            var assemblyPath = Path.Combine(folder, name);

            var assembly = Assembly.LoadFrom(assemblyPath);
            var serviceType = assembly.GetType("Dot42.VStudio.XmlEditor.XmlEditorService", true);
            if (serviceType == null)
                throw new InvalidOperationException("Cannot find XmlEditorService in " + assemblyPath);
            var service = Activator.CreateInstance(serviceType, new object[] {serviceProvider});
            if (service == null)
                throw new InvalidOperationException("Cannot instantiate XmlEditorService in " + assemblyPath);
            editorService = (IXmlEditorService)service;
            return editorService;
        }

    }
}

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Helper methods for working with java class names.
    /// </summary>
    public static class ClassName
    {
        /// <summary>
        /// Gets the package part of the class name?
        /// </summary>
        public static string GetPackage(string className)
        {
            var index = className.LastIndexOf('/');
            return (index < 0) ? string.Empty : className.Substring(0, index);
        }

        /// <summary>
        /// Gets class name without package prefix?
        /// </summary>
        public static string StripPackage(string className)
        {
            if (className == null)
                return null;
            var index = className.LastIndexOf('/');
            return (index < 0) ? className : className.Substring(index + 1);
        }

        /// <summary>
        /// Convert a java class name to a CLR style type name
        /// </summary>
        public static string JavaClassNameToClrTypeName(string className)
        {
            return className.Replace('/', '.').Replace('$', '/');
        }
    }
}

using System;
using Mono.Cecil;

namespace Dot42.CecilExtensions
{
    /// <summary>
    /// Type detection methods
    /// </summary>
    partial class Extensions
    {
        /// <summary>
        /// Is the given type a reference to System.Object?
        /// </summary>
        public static bool IsSystemObject(this TypeReference type)
        {
            return (type.FullName == "System.Object");
        }
    
        /// <summary>
        /// Is the given type a reference to System.Array?
        /// </summary>
        public static bool IsSystemArray(this TypeReference type)
        {
            return (type.FullName == "System.Array");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.ICollection?
        /// </summary>
        public static bool IsSystemCollectionsICollection(this TypeReference type)
        {
            return (type.FullName == "System.Collections.ICollection");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IEnumerable?
        /// </summary>
        public static bool IsSystemCollectionsIEnumerable(this TypeReference type)
        {
            return (type.FullName == "System.Collections.IEnumerable");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IList?
        /// </summary>
        public static bool IsSystemCollectionsIList(this TypeReference type)
        {
            return (type.FullName == "System.Collections.IList");
        }

        /// <summary>
        /// Does the given type extend from System.MulticastDelegate?
        /// </summary>
        public static bool IsDelegate(this TypeDefinition type)
        {
            while (true)
            {
                var baseType = type.BaseType;
                if (baseType == null)
                    return false;
                type = baseType.Resolve();
                if (type.FullName == typeof(MulticastDelegate).FullName)
                {
                    return true;
                }
            }
        }
    }
}

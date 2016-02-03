using System;
using System.Collections.Generic;

namespace Dot42.DexLib
{
    /// <summary>
    /// Helper for quick lookups of classes or methods in a dex file.
    /// Creates a snapshot of all current classes / class names / 
    /// methods / method names / method signatures in the dex.
    /// </summary>
    public class DexLookup
    {
        private readonly Dictionary<string, ClassDefinition> _classesByFullName = new Dictionary<string, ClassDefinition>();
        private readonly Dictionary<Tuple<string, string, string>, MethodDefinition> _methodsBySignature = new Dictionary<Tuple<string, string, string>, MethodDefinition>();

        public DexLookup(Dex dex) : this(dex.Classes, true)
        {
        }

        public DexLookup(IEnumerable<ClassDefinition> classes, bool createMethodLookup)
        {
            foreach (var @class in classes)
            {
                AddClass(@class,createMethodLookup);
            }
        }

        private void AddClass(ClassDefinition @class, bool createMethodLookup = true)
        {
            _classesByFullName[@class.Fullname] = @class;

            if (createMethodLookup)
            {
                foreach (var method in @class.Methods)
                {
                    _methodsBySignature[Tuple.Create(@class.Fullname, method.Name, method.Prototype.ToSignature())] =
                        method;
                }
            }

            foreach(var inner in @class.InnerClasses)
                AddClass(inner, createMethodLookup);
        }

        /// <summary>
        /// returns null if not found
        /// </summary>
        public ClassDefinition GetClass(string fullName)
        {
            ClassDefinition ret;
            _classesByFullName.TryGetValue(fullName, out ret);
            return ret;
        }

        /// <summary>
        /// returns null if not found
        /// </summary>
        public MethodDefinition GetMethod(string classFullName, string methodName, string methodSignature)
        {
            MethodDefinition ret;
            _methodsBySignature.TryGetValue(Tuple.Create(classFullName,methodName,methodSignature), out ret);
            return ret;
        }
    }
}
